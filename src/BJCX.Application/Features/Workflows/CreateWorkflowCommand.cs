using BJCX.Application.Abstractions;
using BJCX.Application.Exceptions;
using BJCX.Domain.Entities;
using FluentValidation;
using MediatR;

namespace BJCX.Application.Features.Workflows;

/// <summary>创建流程时的节点输入。</summary>
public class WorkflowNodeInput
{
    public int Order { get; set; }

    public string Name { get; set; } = string.Empty;

    public long ApproverId { get; set; }
}

/// <summary>创建审批流程命令（简易配置，不引入重型工作流框架）。</summary>
public class CreateWorkflowCommand : IRequest<long>
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public List<WorkflowNodeInput> Nodes { get; set; } = new();
}

/// <summary>创建流程参数校验。</summary>
public class CreateWorkflowCommandValidator : AbstractValidator<CreateWorkflowCommand>
{
    public CreateWorkflowCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("流程名称不能为空");
        RuleFor(x => x.Nodes).NotEmpty().WithMessage("至少配置一个审批节点");
        RuleForEach(x => x.Nodes).ChildRules(node =>
        {
            node.RuleFor(n => n.Name).NotEmpty().WithMessage("节点名称不能为空");
            node.RuleFor(n => n.ApproverId).GreaterThan(0).WithMessage("节点审批人无效");
        });
    }
}

/// <summary>创建流程处理器：校验审批人存在并持久化流程与节点。</summary>
public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, long>
{
    private readonly IRepository<WorkflowDefinition> _workflowRepository;
    private readonly IRepository<User> _userRepository;

    public CreateWorkflowCommandHandler(IRepository<WorkflowDefinition> workflowRepository, IRepository<User> userRepository)
    {
        _workflowRepository = workflowRepository;
        _userRepository = userRepository;
    }

    public async Task<long> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = new WorkflowDefinition
        {
            Name = request.Name,
            Description = request.Description,
            IsEnabled = request.IsEnabled
        };

        foreach (var node in request.Nodes.OrderBy(n => n.Order))
        {
            // 校验审批人存在，避免悬空引用
            _ = await _userRepository.GetByIdAsync(node.ApproverId, cancellationToken)
                ?? throw new BusinessException($"节点「{node.Name}」的审批人不存在");

            workflow.Nodes.Add(new WorkflowNode
            {
                Order = node.Order,
                Name = node.Name,
                ApproverId = node.ApproverId
            });
        }

        await _workflowRepository.AddAsync(workflow, cancellationToken);
        await _workflowRepository.SaveChangesAsync(cancellationToken);

        return workflow.Id;
    }
}
