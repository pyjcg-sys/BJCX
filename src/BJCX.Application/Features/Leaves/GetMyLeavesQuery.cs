using AutoMapper;
using BJCX.Application.Abstractions;
using BJCX.Application.DTOs;
using BJCX.Application.Exceptions;
using BJCX.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Leaves;

/// <summary>查询我提交的请假申请分页列表。</summary>
public class GetMyLeavesQuery : IRequest<PagedResult<LeaveDto>>
{
    /// <summary>页码（从 1 开始）。</summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>每页条数。</summary>
    public int PageSize { get; set; } = 10;
}

/// <summary>我的申请列表参数校验。</summary>
public class GetMyLeavesQueryValidator : AbstractValidator<GetMyLeavesQuery>
{
    public GetMyLeavesQueryValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(0).WithMessage("页码必须大于 0");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("每页条数需在 1 到 100 之间");
    }
}

/// <summary>我的申请列表查询处理器。</summary>
public class GetMyLeavesQueryHandler : IRequestHandler<GetMyLeavesQuery, PagedResult<LeaveDto>>
{
    private readonly IRepository<LeaveRequest> _leaveRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyLeavesQueryHandler(IRepository<LeaveRequest> leaveRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _leaveRepository = leaveRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PagedResult<LeaveDto>> Handle(GetMyLeavesQuery request, CancellationToken cancellationToken)
    {
        var applicantId = _currentUserService.UserId ?? throw new BusinessException("当前未登录");

        var query = _leaveRepository.QueryAsNoTracking().Where(l => l.ApplicantId == applicantId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LeaveDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = _mapper.Map<List<LeaveDto>>(items)
        };
    }
}
