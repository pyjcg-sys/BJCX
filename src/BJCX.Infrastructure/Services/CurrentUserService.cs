using BJCX.Application.Abstractions;

namespace BJCX.Infrastructure.Services;

/// <summary>
/// 当前用户服务：scoped 生命周期，由 Api 层的 CurrentUserFilter 解析 JWT 后写入。
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private long? _userId;
    private string? _userName;
    private string? _displayName;
    private string? _email;

    public long? UserId => _userId;

    public string? UserName => _userName;

    public string? DisplayName => _displayName;

    public string? Email => _email;

    public bool IsAuthenticated => _userId.HasValue;

    public void SetUser(long? userId, string? userName, string? displayName, string? email)
    {
        _userId = userId;
        _userName = userName;
        _displayName = displayName;
        _email = email;
    }
}
