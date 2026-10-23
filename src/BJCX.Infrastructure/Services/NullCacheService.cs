using BJCX.Application.Abstractions;

namespace BJCX.Infrastructure.Services;

/// <summary>
/// 空缓存服务：完全禁用缓存，所有操作都是空实现（读返回 default，写/删直接完成）。
/// 当配置 UseCache=false 时使用，此时每次查询都直接访问数据库。
/// </summary>
public class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
