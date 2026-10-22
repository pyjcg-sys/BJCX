using Autofac;
using AutoMapper;
using BJCX.Application.Mapping;
using FluentValidation;

namespace BJCX.Infrastructure.DependencyInjection;

/// <summary>
/// 应用层 Autofac 模块：注册 AutoMapper 与 FluentValidation 校验器。
/// </summary>
public class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // AutoMapper：注册 MapperConfiguration 与 IMapper
        builder.Register(_ => new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()))
            .SingleInstance();

        builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper())
            .As<IMapper>()
            .InstancePerLifetimeScope();

        // FluentValidation：自动注册 Application 程序集内所有 IValidator<> 实现
        builder.RegisterAssemblyTypes(typeof(MappingProfile).Assembly)
            .AsClosedTypesOf(typeof(IValidator<>))
            .InstancePerLifetimeScope();
    }
}
