namespace BJCX.Application.Abstractions;

/// <summary>
/// 统一缓存服务抽象。默认使用 Redis 实现，测试环境可替换为内存实现。
/// 读操作（Query）通过它缓存结果，写操作（Command）通过它清理缓存。
/// </summary>
public interface ICacheService
{
    /// <summary>读取缓存，key 不存在时返回 default。</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>写入缓存，可指定过期时间。</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>删除单个缓存键。</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>按前缀批量删除缓存键（用于业务写操作后清理某类缓存）。</summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
