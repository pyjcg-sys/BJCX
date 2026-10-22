namespace BJCX.Application.Common;

/// <summary>
/// 缓存键常量：统一管理缓存键命名，避免散落在各 Handler 中。
/// </summary>
public static class CacheKeys
{
    /// <summary>请假单据详情缓存键。</summary>
    public static string LeaveDetail(long leaveRequestId) => $"leave:detail:{leaveRequestId}";

    /// <summary>某用户待办列表缓存键前缀（前缀失效用）。</summary>
    public static string PendingListPrefix(long userId) => $"leave:pending:{userId}:";

    /// <summary>某用户待办列表分页缓存键。</summary>
    public static string PendingList(long userId, int pageIndex, int pageSize) => $"leave:pending:{userId}:{pageIndex}:{pageSize}";
}
