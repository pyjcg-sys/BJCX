using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BJCX.Application.Abstractions;
using BJCX.Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;

namespace BJCX.Infrastructure.Services;

/// <summary>
/// JWT 令牌服务：签发包含用户身份声明的访问令牌。
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
    }

    public string GenerateToken(long userId, string userName, string displayName, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName),
            new("displayName", displayName),
            new(ClaimTypes.Email, email ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
