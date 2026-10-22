using AutoMapper;
using BJCX.Application.Abstractions;
using BJCX.Application.Common;
using BJCX.Application.DTOs;
using BJCX.Application.Exceptions;
using BJCX.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Leaves;

/// <summary>查询请假单据详情（含附件与完整审批记录）。</summary>
public class GetLeaveDetailQuery : IRequest<LeaveDetailDto>
{
    public long LeaveRequestId { get; set; }
}

/// <summary>详情查询参数校验。</summary>
public class GetLeaveDetailQueryValidator : AbstractValidator<GetLeaveDetailQuery>
{
    public GetLeaveDetailQueryValidator()
    {
        RuleFor(x => x.LeaveRequestId).GreaterThan(0).WithMessage("单据 Id 无效");
    }
}

/// <summary>详情查询处理器：优先走 Redis 缓存，未命中再查库并回填缓存。</summary>
public class GetLeaveDetailQueryHandler : IRequestHandler<GetLeaveDetailQuery, LeaveDetailDto>
{
    private readonly IRepository<LeaveRequest> _leaveRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetLeaveDetailQueryHandler(IRepository<LeaveRequest> leaveRepository, ICacheService cacheService, IMapper mapper)
    {
        _leaveRepository = leaveRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<LeaveDetailDto> Handle(GetLeaveDetailQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.LeaveDetail(request.LeaveRequestId);

        var cached = await _cacheService.GetAsync<LeaveDetailDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var leave = await _leaveRepository.QueryAsNoTracking()
            .Include(l => l.Attachments)
            .Include(l => l.ApprovalRecords)
            .FirstOrDefaultAsync(l => l.Id == request.LeaveRequestId, cancellationToken)
            ?? throw new NotFoundException("请假单据不存在");

        var dto = _mapper.Map<LeaveDetailDto>(leave);
        dto.ApprovalRecords = dto.ApprovalRecords.OrderBy(r => r.NodeOrder).ThenBy(r => r.ActionTime).ToList();

        await _cacheService.SetAsync(cacheKey, dto, null, cancellationToken);

        return dto;
    }
}
