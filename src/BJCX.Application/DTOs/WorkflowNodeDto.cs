namespace BJCX.Application.DTOs;

/// <summary>
/// 流程节点 DTO。
/// </summary>
public class WorkflowNodeDto
{
    public long Id { get; set; }

    public int Order { get; set; }

    public string Name { get; set; } = string.Empty;

    public long ApproverId { get; set; }

    public string ApproverName { get; set; } = string.Empty;
}
