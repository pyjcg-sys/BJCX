using AutoMapper;
using BJCX.Application.Abstractions;
using BJCX.Application.DTOs;
using BJCX.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Workflows;

/// <summary>查询审批流程列表。</summary>
public class GetWorkflowsQuery : IRequest<List<WorkflowListItemDto>>
{
}

/// <summary>流程列表查询处理器。</summary>
public class GetWorkflowsQueryHandler : IRequestHandler<GetWorkflowsQuery, List<WorkflowListItemDto>>
{
    private readonly IRepository<WorkflowDefinition> _workflowRepository;
    private readonly IMapper _mapper;

    public GetWorkflowsQueryHandler(IRepository<WorkflowDefinition> workflowRepository, IMapper mapper)
    {
        _workflowRepository = workflowRepository;
        _mapper = mapper;
    }

    public async Task<List<WorkflowListItemDto>> Handle(GetWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var workflows = await _workflowRepository.QueryAsNoTracking()
            .Include(w => w.Nodes)
            .OrderBy(w => w.Id)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<WorkflowListItemDto>>(workflows);
    }
}
