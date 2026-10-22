namespace BJCX.Application.DTOs;

/// <summary>
/// 流程定义详情 DTO（含节点）。
/// </summary>
public class WorkflowDefinitionDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public List<WorkflowNodeDto> Nodes { get; set; } = new();
}
