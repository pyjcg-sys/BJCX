namespace BJCX.Application.DTOs;

/// <summary>
/// 登录结果 DTO。
/// </summary>
public class LoginResultDto
{
    /// <summary>JWT 访问令牌。</summary>
    public string Token { get; set; } = string.Empty;

    public long UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
