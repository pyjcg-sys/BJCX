using AutoMapper;
using BJCX.Application.Abstractions;
using BJCX.Application.DTOs;
using BJCX.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Emails;

/// <summary>查询邮件列表（分页）。</summary>
public class GetEmailsQuery : IRequest<PagedResult<EmailMessageDto>>
{
    /// <summary>页码（从 1 开始）。</summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>每页条数。</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>邮件列表查询参数校验。</summary>
public class GetEmailsQueryValidator : AbstractValidator<GetEmailsQuery>
{
    public GetEmailsQueryValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThan(0).WithMessage("页码必须大于 0");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("每页条数需在 1 到 100 之间");
    }
}

/// <summary>邮件列表查询处理器。</summary>
public class GetEmailsQueryHandler : IRequestHandler<GetEmailsQuery, PagedResult<EmailMessageDto>>
{
    private readonly IRepository<EmailMessage> _emailRepository;
    private readonly IMapper _mapper;

    public GetEmailsQueryHandler(IRepository<EmailMessage> emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<EmailMessageDto>> Handle(GetEmailsQuery request, CancellationToken cancellationToken)
    {
        var query = _emailRepository.QueryAsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(e => e.SentAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailMessageDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = _mapper.Map<List<EmailMessageDto>>(items)
        };
    }
}
