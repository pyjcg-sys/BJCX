using AutoMapper;
using BJCX.Application.Abstractions;
using BJCX.Application.DTOs;
using BJCX.Application.Exceptions;
using BJCX.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Workflows;

/// <summary>查询审批流程详情（含节点与审批人）。</summary>
public class GetWorkflowDetailQuery : IRequest<WorkflowDefinitionDto>
{
    public long WorkflowDefinitionId { get; set; }
}

/// <summary>流程详情查询参数校验。</summary>
public class GetWorkflowDetailQueryValidator : AbstractValidator<GetWorkflowDetailQuery>
{
    public GetWorkflowDetailQueryValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId).GreaterThan(0).WithMessage("流程 Id 无效");
    }
}

/// <summary>流程详情查询处理器。</summary>
public class GetWorkflowDetailQueryHandler : IRequestHandler<GetWorkflowDetailQuery, WorkflowDefinitionDto>
{
    private readonly IRepository<WorkflowDefinition> _workflowRepository;
    private readonly IMapper _mapper;

    public GetWorkflowDetailQueryHandler(IRepository<WorkflowDefinition> workflowRepository, IMapper mapper)
    {
        _workflowRepository = workflowRepository;
        _mapper = mapper;
    }

    public async Task<WorkflowDefinitionDto> Handle(GetWorkflowDetailQuery request, CancellationToken cancellationToken)
    {
        var workflow = await _workflowRepository.QueryAsNoTracking()
            .Include(w => w.Nodes).ThenInclude(n => n.Approver)
            .FirstOrDefaultAsync(w => w.Id == request.WorkflowDefinitionId, cancellationToken)
            ?? throw new NotFoundException("审批流程不存在");

        return _mapper.Map<WorkflowDefinitionDto>(workflow);
    }
}
