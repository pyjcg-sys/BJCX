using AutoMapper;
using BJCX.Application.Abstractions;
using BJCX.Application.Mapping;
using BJCX.Domain.Entities;
using BJCX.Domain.Security;
using BJCX.Infrastructure.Options;
using BJCX.Infrastructure.Persistence;
using BJCX.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BJCX.Tests;

/// <summary>
/// 测试基类：提供内存 SQLite、仓储、映射器、内存缓存、邮件 Mock 与种子数据。
/// 每个测试方法获得独立的数据库实例，避免状态污染。
/// </summary>
public abstract class TestBase : IDisposable
{
    private readonly SqliteConnection _connection;

    protected AppDbContext DbContext { get; }

    protected IMapper Mapper { get; }

    protected ICacheService CacheService { get; }

    protected Mock<IEmailService> EmailServiceMock { get; }

    protected long ApplicantId { get; private set; }

    protected long LeaderId { get; private set; }

    protected long ManagerId { get; private set; }

    protected TestBase()
    {
        // 内存 SQLite：保持连接打开以维持数据库存活
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new AppDbContext(options);
        DbContext.Database.EnsureCreated();

        Mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        CacheService = new InMemoryCacheService(new RedisOptions { DefaultExpiryMinutes = 10 });
        EmailServiceMock = new Mock<IEmailService>();

        Seed();
    }

    /// <summary>创建指定用户身份的当前用户上下文。</summary>
    protected ICurrentUserService CreateCurrentUser(long userId, string userName, string displayName)
    {
        var service = new CurrentUserService();
        service.SetUser(userId, userName, displayName, $"{userName}@bjcx.com");
        return service;
    }

    /// <summary>写入种子数据：申请人/直属领导/部门经理 + 默认两级审批流程。</summary>
    private void Seed()
    {
        var applicant = CreateUser("zhangsan", "张三", "E001");
        var leader = CreateUser("lisi", "李四", "E002");
        var manager = CreateUser("wangwu", "王五", "E003");

        DbContext.Users.AddRange(applicant, leader, manager);
        DbContext.SaveChanges();

        ApplicantId = applicant.Id;
        LeaderId = leader.Id;
        ManagerId = manager.Id;

        var workflow = new WorkflowDefinition
        {
            Name = "请假审批流程",
            Description = "测试流程：直属领导 → 部门经理",
            IsEnabled = true
        };
        workflow.Nodes.Add(new WorkflowNode { Order = 0, Name = "直属领导审批", ApproverId = leader.Id });
        workflow.Nodes.Add(new WorkflowNode { Order = 1, Name = "部门经理审批", ApproverId = manager.Id });

        DbContext.WorkflowDefinitions.Add(workflow);
        DbContext.SaveChanges();
    }

    private static User CreateUser(string userName, string displayName, string employeeNo) => new()
    {
        UserName = userName,
        DisplayName = displayName,
        Email = $"{userName}@bjcx.com",
        EmployeeNo = employeeNo,
        Department = "研发部",
        PasswordHash = PasswordHasher.Hash("123456"),
        IsActive = true
    };

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
    }
}
