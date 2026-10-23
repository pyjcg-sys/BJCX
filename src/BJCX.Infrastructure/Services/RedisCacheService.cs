using System.Text.Json;
using BJCX.Application.Abstractions;
using BJCX.Infrastructure.Options;
using StackExchange.Redis;

namespace BJCX.Infrastructure.Services;

/// <summary>
/// Redis 缓存服务：基于 StackExchange.Redis，值为 JSON 序列化字符串。
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly TimeSpan _defaultExpiry;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(IConnectionMultiplexer redis, RedisOptions options)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _defaultExpiry = TimeSpan.FromMinutes(options.DefaultExpiryMinutes);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await _database.StringSetAsync(key, json, expiry ?? _defaultExpiry);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync(key);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // 使用 SCAN 逐批删除，避免 KEYS 在大数据集下阻塞 Redis
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        await foreach (var key in server.KeysAsync(pattern: $"{prefix}*", pageSize: 100))
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
