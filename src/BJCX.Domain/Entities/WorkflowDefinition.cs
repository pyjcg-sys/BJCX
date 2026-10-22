namespace BJCX.Domain.Entities;

/// <summary>
/// 审批流程配置（支持自由配置流程节点与审批人）。
/// </summary>
public class WorkflowDefinition : BaseEntity
{
    /// <summary>流程名称，如「请假审批流程」。</summary>
    public string Name { get; set; } = null!;

    /// <summary>流程描述。</summary>
    public string Description { get; set; } = null!;

    /// <summary>是否启用（提交单据时默认选择启用的流程）。</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>流程节点集合（按 Order 串行流转）。</summary>
    public ICollection<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();
}
