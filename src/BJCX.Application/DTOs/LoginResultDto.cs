namespace BJCX.Application.DTOs;

/// <summary>
/// 登录结果 DTO。
/// </summary>
public class LoginResultDto
{
    /// <summary>JWT 访问令牌。</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>用户 Id。</summary>
    public long UserId { get; set; }

    /// <summary>登录名。</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>显示名。</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>邮箱。</summary>
    public string Email { get; set; } = string.Empty;
}
