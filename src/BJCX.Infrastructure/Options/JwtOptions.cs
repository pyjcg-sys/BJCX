namespace BJCX.Infrastructure.Options;

/// <summary>
/// JWT 配置项（对应 appsettings.json 的 Jwt 节）。
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>签发者。</summary>
    public string Issuer { get; set; } = "BJCX";

    /// <summary>受众。</summary>
    public string Audience { get; set; } = "BJCX.Api";

    /// <summary>签名密钥（生产环境应使用足够长的随机字符串）。</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>令牌有效期（分钟）。</summary>
    public int ExpiresMinutes { get; set; } = 60;
}
