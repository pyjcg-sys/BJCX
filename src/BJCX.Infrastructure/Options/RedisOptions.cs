namespace BJCX.Infrastructure.Options;

/// <summary>
/// Redis 缓存配置项（对应 appsettings.json 的 RedisConfig 节）。
/// </summary>
public class RedisOptions
{
    public const string SectionName = "RedisConfig";

    /// <summary>是否启用 Redis 缓存（false 时完全禁用缓存，直查数据库）。</summary>
    public bool UseCache { get; set; } = true;

    /// <summary>Redis 连接串（含 DefaultDatabase 指定 db 号与超时参数）。</summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>默认缓存过期时间（分钟）。</summary>
    public int DefaultExpiryMinutes { get; set; } = 10;
}
