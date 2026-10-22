using Autofac;
using BJCX.Application.Abstractions;
using BJCX.Domain.Entities;
using BJCX.Infrastructure.Options;
using BJCX.Infrastructure.Persistence;
using BJCX.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using StackExchange.Redis;

namespace BJCX.Infrastructure.DependencyInjection;

/// <summary>
/// 基础设施层 Autofac 模块：注册仓储、缓存、文件存储、邮件、当前用户、令牌等服务。
/// </summary>
public class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // 配置项：从 IConfiguration 绑定 Jwt 与 Cache 节
        builder.Register(ctx =>
        {
            var options = new JwtOptions();
            ctx.Resolve<IConfiguration>().GetSection(JwtOptions.SectionName).Bind(options);
            return options;
        }).SingleInstance();

        builder.Register(ctx =>
        {
            var options = new CacheOptions();
            ctx.Resolve<IConfiguration>().GetSection(CacheOptions.SectionName).Bind(options);
            return options;
        }).SingleInstance();

        // Redis 连接复用器（单例，线程安全）
        builder.Register(ctx =>
        {
            var options = ctx.Resolve<CacheOptions>();
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        }).As<IConnectionMultiplexer>().SingleInstance();

        // 缓存服务：根据配置切换 Redis / 内存实现
        builder.Register(ctx =>
        {
            var options = ctx.Resolve<CacheOptions>();
            if (string.Equals(options.Provider, "Memory", StringComparison.OrdinalIgnoreCase))
            {
                return (ICacheService)new InMemoryCacheService(options);
            }

            return new RedisCacheService(ctx.Resolve<IConnectionMultiplexer>(), options);
        }).As<ICacheService>().InstancePerLifetimeScope();

        // 通用仓储
        builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

        // 各类服务实现
        builder.RegisterType<LocalFileStorageService>().As<IFileStorageService>().InstancePerLifetimeScope();
        builder.RegisterType<LogEmailService>().As<IEmailService>().SingleInstance();
        builder.RegisterType<CurrentUserService>().As<ICurrentUserService>().InstancePerLifetimeScope();
        builder.RegisterType<JwtTokenService>().As<ITokenService>().InstancePerLifetimeScope();

        // Serilog 日志器（供邮件服务等注入）
        builder.Register(_ => Log.Logger).As<ILogger>().SingleInstance();
    }
}
