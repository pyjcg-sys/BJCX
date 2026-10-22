namespace BJCX.Application.Abstractions;

/// <summary>
/// JWT 令牌签发服务抽象。登录成功后由它生成访问令牌。
/// </summary>
public interface ITokenService
{
    /// <summary>根据用户信息签发 JWT。</summary>
    string GenerateToken(long userId, string userName, string displayName, string email);
}
