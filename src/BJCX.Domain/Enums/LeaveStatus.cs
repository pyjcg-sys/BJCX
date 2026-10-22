namespace BJCX.Domain.Enums;

/// <summary>
/// 请假单据状态。
/// </summary>
public enum LeaveStatus
{
    /// <summary>审批中。</summary>
    Pending = 1,

    /// <summary>已通过。</summary>
    Approved = 2,

    /// <summary>已驳回。</summary>
    Rejected = 3
}
