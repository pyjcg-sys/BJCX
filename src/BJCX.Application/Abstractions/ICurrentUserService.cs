namespace BJCX.Application.Abstractions;

/// <summary>
/// 当前登录用户上下文。由 Api 层的 CurrentUserFilter 从 JWT 解析后写入，
/// Handler 只读取它获取当前操作人，不在业务代码里重复解析 Token。
/// </summary>
public interface ICurrentUserService
{
    /// <summary>当前用户 Id（未登录时为 null）。</summary>
    long? UserId { get; }

    /// <summary>当前用户登录名。</summary>
    string? UserName { get; }

    /// <summary>当前用户显示名。</summary>
    string? DisplayName { get; }

    /// <summary>当前用户邮箱。</summary>
    string? Email { get; }

    /// <summary>是否已通过认证。</summary>
    bool IsAuthenticated { get; }
}
