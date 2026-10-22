namespace BJCX.Domain.Entities;

/// <summary>
/// 流程节点：一个审批环节，串行执行，每个节点指定一名审批人（简易实现）。
/// </summary>
public class WorkflowNode : BaseEntity
{
    /// <summary>所属流程定义 Id。</summary>
    public long WorkflowDefinitionId { get; set; }

    /// <summary>所属流程定义。</summary>
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    /// <summary>节点序号（从 0 开始，串行流转顺序）。</summary>
    public int Order { get; set; }

    /// <summary>节点名称，如「直属领导审批」。</summary>
    public string Name { get; set; } = null!;

    /// <summary>审批人 Id。</summary>
    public long ApproverId { get; set; }

    /// <summary>审批人。</summary>
    public User Approver { get; set; } = null!;
}
