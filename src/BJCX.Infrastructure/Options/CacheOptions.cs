namespace BJCX.Infrastructure.Options;

/// <summary>
/// 缓存配置项（对应 appsettings.json 的 Cache 节）。
/// </summary>
public class CacheOptions
{
    public const string SectionName = "Cache";

    /// <summary>缓存提供方：Redis 或 Memory（内存实现，用于测试）。</summary>
    public string Provider { get; set; } = "Redis";

    /// <summary>Redis 连接串。</summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>默认缓存过期时间（分钟）。</summary>
    public int DefaultExpiryMinutes { get; set; } = 10;
}
