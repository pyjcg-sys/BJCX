using System.Collections.Concurrent;
using BJCX.Application.Abstractions;
using BJCX.Infrastructure.Options;

namespace BJCX.Infrastructure.Services;

/// <summary>
/// 内存缓存服务：基于 ConcurrentDictionary，用于单元测试或无需 Redis 的演示场景。
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultExpiry;

    public InMemoryCacheService(RedisOptions options)
    {
        _defaultExpiry = TimeSpan.FromMinutes(options.DefaultExpiryMinutes);
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
        {
            return Task.FromResult((T?)entry.Value);
        }

        _cache.TryRemove(key, out _);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        _cache[key] = new CacheEntry(value, DateTimeOffset.UtcNow.Add(expiry ?? _defaultExpiry));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)))
        {
            _cache.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>缓存条目，携带过期时间。</summary>
    private sealed class CacheEntry
    {
        public CacheEntry(object? value, DateTimeOffset expiresAt)
        {
            Value = value;
            ExpiresAt = expiresAt;
        }

        public object? Value { get; }

        public DateTimeOffset ExpiresAt { get; }

        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    }
}
