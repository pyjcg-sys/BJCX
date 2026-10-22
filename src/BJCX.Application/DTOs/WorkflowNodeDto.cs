namespace BJCX.Application.DTOs;

/// <summary>
/// 流程节点 DTO。
/// </summary>
public class WorkflowNodeDto
{
    /// <summary>节点 Id。</summary>
    public long Id { get; set; }

    /// <summary>节点序号（串行流转顺序）。</summary>
    public int Order { get; set; }

    /// <summary>节点名称。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>审批人 Id。</summary>
    public long ApproverId { get; set; }

    /// <summary>审批人姓名。</summary>
    public string ApproverName { get; set; } = string.Empty;
}
