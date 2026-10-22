namespace BJCX.Domain.Enums;

/// <summary>
/// 审批节点的处理动作。
/// </summary>
public enum ApprovalAction
{
    /// <summary>待审批（节点已创建、尚未处理）。</summary>
    Pending = 0,

    /// <summary>已同意。</summary>
    Approved = 1,

    /// <summary>已驳回。</summary>
    Rejected = 2
}
