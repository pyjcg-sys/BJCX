namespace BJCX.Application.DTOs;

/// <summary>
/// 流程定义列表项 DTO。
/// </summary>
public class WorkflowListItemDto
{
    /// <summary>流程 Id。</summary>
    public long Id { get; set; }

    /// <summary>流程名称。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>流程描述。</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>是否启用。</summary>
    public bool IsEnabled { get; set; }

    /// <summary>节点数量。</summary>
    public int NodeCount { get; set; }
}
