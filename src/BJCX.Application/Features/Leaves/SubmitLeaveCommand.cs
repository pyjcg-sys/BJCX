using BJCX.Application.Abstractions;
using BJCX.Application.Common;
using BJCX.Application.Exceptions;
using BJCX.Domain.Entities;
using BJCX.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BJCX.Application.Features.Leaves;

/// <summary>提交时已上传附件的元信息（文件体已由文件存储服务落盘）。</summary>
public class AttachmentUploadDto
{
    /// <summary>原始文件名。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>存储 Key（文件存储服务内唯一标识）。</summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>内容类型（MIME）。</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>文件大小（字节）。</summary>
    public long FileSize { get; set; }
}

/// <summary>提交请假申请命令（申请人从当前登录用户解析）。</summary>
public class SubmitLeaveCommand : IRequest<long>
{
    /// <summary>请假类型（Annual/Personal/Sick/Marriage/Other）。</summary>
    public LeaveType LeaveType { get; set; }

    /// <summary>开始日期。</summary>
    public DateTime StartDate { get; set; }

    /// <summary>结束日期。</summary>
    public DateTime EndDate { get; set; }

    /// <summary>请假事由。</summary>
    public string? Reason { get; set; }

    /// <summary>已上传的附件元信息列表。</summary>
    public List<AttachmentUploadDto> Attachments { get; set; } = new();
}

/// <summary>提交申请参数校验。</summary>
public class SubmitLeaveCommandValidator : AbstractValidator<SubmitLeaveCommand>
{
    public SubmitLeaveCommandValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("开始日期不能为空");
        RuleFor(x => x.EndDate).NotEmpty().WithMessage("结束日期不能为空");
        RuleFor(x => x).Must(x => x.EndDate.Date >= x.StartDate.Date).WithMessage("结束日期不能早于开始日期");
        RuleFor(x => x.Reason).NotEmpty().When(x => x.LeaveType == LeaveType.Other).WithMessage("其他类型请假必须填写事由");
        RuleFor(x => x.Attachments).Must(a => a.Count <= 10).WithMessage("附件数量不能超过 10 个");
    }
}

/// <summary>提交请假申请处理器：创建单据与首节点待办、发通知邮件、清理待办缓存。</summary>
public class SubmitLeaveCommandHandler : IRequestHandler<SubmitLeaveCommand, long>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<WorkflowDefinition> _workflowRepository;
    private readonly IRepository<LeaveRequest> _leaveRepository;
    private readonly IRepository<LeaveAttachment> _attachmentRepository;
    private readonly IRepository<ApprovalRecord> _approvalRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ILogger _logger;

    public SubmitLeaveCommandHandler(
        IRepository<User> userRepository,
        IRepository<WorkflowDefinition> workflowRepository,
        IRepository<LeaveRequest> leaveRepository,
        IRepository<LeaveAttachment> attachmentRepository,
        IRepository<ApprovalRecord> approvalRepository,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ICacheService cacheService,
        ILogger logger)
    {
        _userRepository = userRepository;
        _workflowRepository = workflowRepository;
        _leaveRepository = leaveRepository;
        _attachmentRepository = attachmentRepository;
        _approvalRepository = approvalRepository;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<long> Handle(SubmitLeaveCommand request, CancellationToken cancellationToken)
    {
        var applicantId = _currentUserService.UserId
            ?? throw new BusinessException("当前未登录，无法提交申请");

        var applicant = await _userRepository.GetByIdAsync(applicantId, cancellationToken)
            ?? throw new BusinessException("申请人不存在");

        // 选择启用的默认审批流程（含节点与审批人）
        var workflow = await _workflowRepository.QueryAsNoTracking()
            .Include(w => w.Nodes).ThenInclude(n => n.Approver)
            .Where(w => w.IsEnabled)
            .OrderBy(w => w.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BusinessException("未配置可用的审批流程");

        var nodes = workflow.Nodes.OrderBy(n => n.Order).ToList();
        if (nodes.Count == 0)
        {
            throw new BusinessException("审批流程未配置节点");
        }

        // 计算请假天数（含首尾两天）
        var days = (request.EndDate.Date - request.StartDate.Date).Days + 1;

        var leave = new LeaveRequest
        {
            RequestNo = GenerateRequestNo(),
            ApplicantId = applicantId,
            ApplicantName = applicant.DisplayName,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = days,
            Reason = request.Reason,
            WorkflowDefinitionId = workflow.Id,
            CurrentNodeOrder = 0,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _leaveRepository.AddAsync(leave, cancellationToken);
        await _leaveRepository.SaveChangesAsync(cancellationToken); // 先保存以取得自增主键

        // 保存附件元数据
        foreach (var attachment in request.Attachments)
        {
            await _attachmentRepository.AddAsync(new LeaveAttachment
            {
                LeaveRequestId = leave.Id,
                FileName = attachment.FileName,
                StorageKey = attachment.StorageKey,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        // 创建第一个审批节点的待办记录
        var firstNode = nodes[0];
        await _approvalRepository.AddAsync(new ApprovalRecord
        {
            LeaveRequestId = leave.Id,
            NodeOrder = firstNode.Order,
            NodeName = firstNode.Name,
            ApproverId = firstNode.ApproverId,
            ApproverName = firstNode.Approver?.DisplayName ?? string.Empty,
            Action = ApprovalAction.Pending,
            ActionTime = DateTime.UtcNow
        }, cancellationToken);

        await _approvalRepository.SaveChangesAsync(cancellationToken);

        // 通知第一个审批人
        if (!string.IsNullOrEmpty(firstNode.Approver?.Email))
        {
            await _emailService.SendAsync(
                new EmailRecipient(firstNode.ApproverId, firstNode.Approver.DisplayName, firstNode.Approver.Email),
                "【请假审批】新的待办申请",
                $"员工 {applicant.DisplayName} 提交了请假申请（单号：{leave.RequestNo}，{request.StartDate:yyyy-MM-dd} 至 {request.EndDate:yyyy-MM-dd}），请及时审批。",
                cancellationToken);
        }

        // 清理第一个审批人的待办列表缓存
        await _cacheService.RemoveByPrefixAsync(CacheKeys.PendingListPrefix(firstNode.ApproverId), cancellationToken);

        _logger.ForContext("RequestNo", leave.RequestNo).Information("提交请假申请成功，单据编号 {RequestNo}", leave.RequestNo);

        return leave.Id;
    }

    /// <summary>生成单据编号：LX-yyyyMMdd-6 位随机大写。</summary>
    private static string GenerateRequestNo()
        => $"LX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
}
