namespace BJCX.Application.DTOs;

/// <summary>
/// 流程定义列表项 DTO。
/// </summary>
public class WorkflowListItemDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    /// <summary>节点数量。</summary>
    public int NodeCount { get; set; }
}
