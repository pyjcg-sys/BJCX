using AutoMapper;
using BJCX.Application.Abstractions;
using BJCX.Application.Common;
using BJCX.Application.DTOs;
using BJCX.Application.Exceptions;
using BJCX.Domain.Entities;
using BJCX.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Leaves;

/// <summary>查询我的待审批单据分页列表。</summary>
public class GetMyPendingListQuery : IRequest<PagedResult<LeaveDto>>
{
    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}

/// <summary>待办列表查询参数校验。</summary>
public class GetMyPendingListQueryValidator : AbstractValidator<GetMyPendingListQuery>
{
    public GetMyPendingListQueryValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(0).WithMessage("页码必须大于 0");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("每页条数需在 1 到 100 之间");
    }
}

/// <summary>待办列表查询处理器：按当前登录人过滤，分页结果走 Redis 缓存。</summary>
public class GetMyPendingListQueryHandler : IRequestHandler<GetMyPendingListQuery, PagedResult<LeaveDto>>
{
    private readonly IRepository<ApprovalRecord> _approvalRepository;
    private readonly IRepository<LeaveRequest> _leaveRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetMyPendingListQueryHandler(
        IRepository<ApprovalRecord> approvalRepository,
        IRepository<LeaveRequest> leaveRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IMapper mapper)
    {
        _approvalRepository = approvalRepository;
        _leaveRepository = leaveRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<PagedResult<LeaveDto>> Handle(GetMyPendingListQuery request, CancellationToken cancellationToken)
    {
        var operatorId = _currentUserService.UserId
            ?? throw new BusinessException("当前未登录");

        var cacheKey = CacheKeys.PendingList(operatorId, request.PageIndex, request.PageSize);

        var cached = await _cacheService.GetAsync<PagedResult<LeaveDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        // 当前用户作为审批人、节点处于待办、且单据审批中的记录
        var query =
            from ar in _approvalRepository.QueryAsNoTracking()
            join lr in _leaveRepository.QueryAsNoTracking() on ar.LeaveRequestId equals lr.Id
            where ar.ApproverId == operatorId
                  && ar.Action == ApprovalAction.Pending
                  && ar.NodeOrder == lr.CurrentNodeOrder
                  && lr.Status == LeaveStatus.Pending
            select lr;

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<LeaveDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = _mapper.Map<List<LeaveDto>>(items)
        };

        await _cacheService.SetAsync(cacheKey, result, null, cancellationToken);

        return result;
    }
}
