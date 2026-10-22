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

/// <summary>审批驳回命令。</summary>
public class RejectLeaveCommand : IRequest
{
    /// <summary>请假单据 Id。</summary>
    public long LeaveRequestId { get; set; }

    /// <summary>驳回意见（必填）。</summary>
    public string Comment { get; set; } = string.Empty;
}

/// <summary>审批驳回参数校验。</summary>
public class RejectLeaveCommandValidator : AbstractValidator<RejectLeaveCommand>
{
    public RejectLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId).GreaterThan(0).WithMessage("单据 Id 无效");
        RuleFor(x => x.Comment).NotEmpty().WithMessage("驳回时必须填写驳回意见");
        RuleFor(x => x.Comment).MaximumLength(500).WithMessage("驳回意见不能超过 500 字");
    }
}

/// <summary>审批驳回处理器：校验权限与状态，驳回单据并通知申请人。</summary>
public class RejectLeaveCommandHandler : IRequestHandler<RejectLeaveCommand>
{
    private readonly IRepository<LeaveRequest> _leaveRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ILogger _logger;

    public RejectLeaveCommandHandler(
        IRepository<LeaveRequest> leaveRepository,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ICacheService cacheService,
        ILogger logger)
    {
        _leaveRepository = leaveRepository;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(RejectLeaveCommand request, CancellationToken cancellationToken)
    {
        var operatorId = _currentUserService.UserId
            ?? throw new BusinessException("当前未登录，无法审批");

        var leave = await _leaveRepository.Query()
            .Include(l => l.ApprovalRecords)
            .Include(l => l.Applicant)
            .FirstOrDefaultAsync(l => l.Id == request.LeaveRequestId, cancellationToken)
            ?? throw new NotFoundException("请假单据不存在");

        if (leave.Status != LeaveStatus.Pending)
        {
            throw new BusinessException("单据当前状态不允许审批");
        }

        var currentNode = leave.ApprovalRecords.FirstOrDefault(r => r.NodeOrder == leave.CurrentNodeOrder && r.Action == ApprovalAction.Pending)
            ?? throw new BusinessException("当前审批节点不存在");

        if (currentNode.ApproverId != operatorId)
        {
            throw new BusinessException("您不是当前节点的审批人，无权限操作");
        }

        // 驳回：当前节点标记为驳回，单据状态置为驳回
        currentNode.Action = ApprovalAction.Rejected;
        currentNode.Comment = request.Comment;
        currentNode.ActionTime = DateTime.UtcNow;

        leave.Status = LeaveStatus.Rejected;
        leave.CurrentNodeOrder = -1;
        leave.UpdatedAt = DateTime.UtcNow;

        await _leaveRepository.SaveChangesAsync(cancellationToken);

        // 通知申请人驳回结果与意见
        if (!string.IsNullOrEmpty(leave.Applicant?.Email))
        {
            await _emailService.SendAsync(
                leave.Applicant.Email,
                "【请假审批】申请被驳回",
                $"您的请假申请（单号：{leave.RequestNo}）已被驳回。驳回意见：{request.Comment}",
                cancellationToken);
        }

        // 清理当前审批人待办缓存与详情缓存
        await _cacheService.RemoveByPrefixAsync(CacheKeys.PendingListPrefix(operatorId), cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.LeaveDetail(leave.Id), cancellationToken);

        _logger.ForContext("RequestNo", leave.RequestNo).Information("审批驳回操作成功，单据编号 {RequestNo}", leave.RequestNo);
    }
}
