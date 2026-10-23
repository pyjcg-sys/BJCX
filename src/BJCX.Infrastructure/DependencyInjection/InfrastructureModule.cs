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
        // 配置项：从 IConfiguration 绑定 Jwt 与 RedisConfig 节
        builder.Register(ctx =>
        {
            var options = new JwtOptions();
            ctx.Resolve<IConfiguration>().GetSection(JwtOptions.SectionName).Bind(options);
            return options;
        }).SingleInstance();

        builder.Register(ctx =>
        {
            var options = new RedisOptions();
            ctx.Resolve<IConfiguration>().GetSection(RedisOptions.SectionName).Bind(options);
            return options;
        }).SingleInstance();

        // Redis 连接复用器（单例，线程安全；UseCache=false 时不会被解析，也就不连接 Redis）
        builder.Register(ctx =>
        {
            var options = ctx.Resolve<RedisOptions>();
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        }).As<IConnectionMultiplexer>().SingleInstance();

        // 缓存服务：UseCache=true 用 Redis，false 完全禁用缓存（NullCacheService）
        builder.Register(ctx =>
        {
            var options = ctx.Resolve<RedisOptions>();
            if (options.UseCache)
            {
                return (ICacheService)new RedisCacheService(ctx.Resolve<IConnectionMultiplexer>(), options);
            }

            return (ICacheService)new NullCacheService();
        }).As<ICacheService>().SingleInstance();

        // 通用仓储
        builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

        // 工作单元（事务）
        builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

        // 各类服务实现
        builder.RegisterType<LocalFileStorageService>().As<IFileStorageService>().InstancePerLifetimeScope();
        builder.RegisterType<LogEmailService>().As<IEmailService>().SingleInstance();
        builder.RegisterType<CurrentUserService>().As<ICurrentUserService>().InstancePerLifetimeScope();
        builder.RegisterType<JwtTokenService>().As<ITokenService>().InstancePerLifetimeScope();

        // Serilog 日志器（供邮件服务等注入）
        builder.Register(_ => Log.Logger).As<ILogger>().SingleInstance();
    }
}
