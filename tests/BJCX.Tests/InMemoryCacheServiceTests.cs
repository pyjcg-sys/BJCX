using BJCX.Infrastructure.Options;
using BJCX.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BJCX.Tests;

/// <summary>内存缓存服务单元测试：验证读写、过期与前缀删除。</summary>
public class InMemoryCacheServiceTests
{
    private static InMemoryCacheService CreateCache() => new(new RedisOptions { DefaultExpiryMinutes = 10 });

    [Fact]
    public async Task 写入后能读取到值()
    {
        var cache = CreateCache();

        await cache.SetAsync("key", "value");
        var result = await cache.GetAsync<string>("key");

        result.Should().Be("value");
    }

    [Fact]
    public async Task 前缀删除只清理匹配的缓存键()
    {
        var cache = CreateCache();

        await cache.SetAsync("leave:detail:1", "a");
        await cache.SetAsync("leave:detail:2", "b");
        await cache.SetAsync("leave:pending:3:1:10", "c");

        await cache.RemoveByPrefixAsync("leave:detail:");

        (await cache.GetAsync<string>("leave:detail:1")).Should().BeNull();
        (await cache.GetAsync<string>("leave:detail:2")).Should().BeNull();
        (await cache.GetAsync<string>("leave:pending:3:1:10")).Should().Be("c");
    }

    [Fact]
    public async Task 单键删除不影响其他键()
    {
        var cache = CreateCache();

        await cache.SetAsync("a", 1);
        await cache.SetAsync("b", 2);

        await cache.RemoveAsync("a");

        (await cache.GetAsync<int?>("a")).Should().BeNull();
        (await cache.GetAsync<int?>("b")).Should().Be(2);
    }
}
