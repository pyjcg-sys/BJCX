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

/// <summary>审批同意命令。</summary>
public class ApproveLeaveCommand : IRequest
{
    public long LeaveRequestId { get; set; }

    public string Comment { get; set; } = string.Empty;
}

/// <summary>审批同意参数校验。</summary>
public class ApproveLeaveCommandValidator : AbstractValidator<ApproveLeaveCommand>
{
    public ApproveLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId).GreaterThan(0).WithMessage("单据 Id 无效");
        RuleFor(x => x.Comment).MaximumLength(500).WithMessage("审批意见不能超过 500 字");
    }
}

/// <summary>审批同意处理器：校验权限与状态，按串行规则流转到下一节点或通过。</summary>
public class ApproveLeaveCommandHandler : IRequestHandler<ApproveLeaveCommand>
{
    private readonly IRepository<LeaveRequest> _leaveRepository;
    private readonly IRepository<ApprovalRecord> _approvalRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ILogger _logger;

    public ApproveLeaveCommandHandler(
        IRepository<LeaveRequest> leaveRepository,
        IRepository<ApprovalRecord> approvalRepository,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ICacheService cacheService,
        ILogger logger)
    {
        _leaveRepository = leaveRepository;
        _approvalRepository = approvalRepository;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(ApproveLeaveCommand request, CancellationToken cancellationToken)
    {
        var operatorId = _currentUserService.UserId
            ?? throw new BusinessException("当前未登录，无法审批");

        var leave = await _leaveRepository.Query()
            .Include(l => l.ApprovalRecords)
            .Include(l => l.Applicant)
            .Include(l => l.WorkflowDefinition).ThenInclude(w => w.Nodes).ThenInclude(n => n.Approver)
            .FirstOrDefaultAsync(l => l.Id == request.LeaveRequestId, cancellationToken)
            ?? throw new NotFoundException("请假单据不存在");

        // 校验单据状态
        if (leave.Status != LeaveStatus.Pending)
        {
            throw new BusinessException("单据当前状态不允许审批");
        }

        // 定位当前待办节点
        var currentNode = leave.ApprovalRecords.FirstOrDefault(r => r.NodeOrder == leave.CurrentNodeOrder && r.Action == ApprovalAction.Pending)
            ?? throw new BusinessException("当前审批节点不存在");

        // 校验操作人权限
        if (currentNode.ApproverId != operatorId)
        {
            throw new BusinessException("您不是当前节点的审批人，无权限操作");
        }

        // 当前节点标记为已同意
        currentNode.Action = ApprovalAction.Approved;
        currentNode.Comment = request.Comment;
        currentNode.ActionTime = DateTime.UtcNow;

        var nodes = leave.WorkflowDefinition.Nodes.OrderBy(n => n.Order).ToList();
        var nextNode = nodes.FirstOrDefault(n => n.Order > leave.CurrentNodeOrder);

        if (nextNode is not null)
        {
            // 流转到下一节点
            leave.CurrentNodeOrder = nextNode.Order;
            leave.UpdatedAt = DateTime.UtcNow;

            await _approvalRepository.AddAsync(new ApprovalRecord
            {
                LeaveRequestId = leave.Id,
                NodeOrder = nextNode.Order,
                NodeName = nextNode.Name,
                ApproverId = nextNode.ApproverId,
                ApproverName = nextNode.Approver?.DisplayName ?? string.Empty,
                Action = ApprovalAction.Pending,
                ActionTime = DateTime.UtcNow
            }, cancellationToken);

            if (!string.IsNullOrEmpty(nextNode.Approver?.Email))
            {
                await _emailService.SendAsync(
                    nextNode.Approver.Email,
                    "【请假审批】新的待办申请",
                    $"员工 {leave.ApplicantName} 的请假申请（单号：{leave.RequestNo}）已由上一节点审批通过，请及时审批。",
                    cancellationToken);
            }

            // 清理当前审批人与下一审批人的待办缓存
            await _cacheService.RemoveByPrefixAsync(CacheKeys.PendingListPrefix(operatorId), cancellationToken);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.PendingListPrefix(nextNode.ApproverId), cancellationToken);
        }
        else
        {
            // 无下一节点：单据通过
            leave.Status = LeaveStatus.Approved;
            leave.CurrentNodeOrder = -1;
            leave.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(leave.Applicant?.Email))
            {
                await _emailService.SendAsync(
                    leave.Applicant.Email,
                    "【请假审批】申请已通过",
                    $"您的请假申请（单号：{leave.RequestNo}）已全部审批通过。",
                    cancellationToken);
            }

            // 清理当前审批人的待办缓存
            await _cacheService.RemoveByPrefixAsync(CacheKeys.PendingListPrefix(operatorId), cancellationToken);
        }

        await _leaveRepository.SaveChangesAsync(cancellationToken);

        // 清理详情缓存
        await _cacheService.RemoveAsync(CacheKeys.LeaveDetail(leave.Id), cancellationToken);

        _logger.ForContext("RequestNo", leave.RequestNo).Information("审批同意操作成功，单据编号 {RequestNo}", leave.RequestNo);
    }
}
