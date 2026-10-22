using BJCX.Domain.Entities;
using BJCX.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Infrastructure.Persistence;

/// <summary>
/// 数据库初始化：确保数据库创建并写入种子数据（演示用户与默认审批流程）。
/// </summary>
public static class DbInitializer
{
    /// <summary>默认演示密码（仅用于本地开发/演示）。</summary>
    public const string DefaultPassword = "123456";

    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        // 使用 EnsureCreated 建库，规避对 dotnet-ef 工具的依赖
        await dbContext.Database.EnsureCreatedAsync();

        // 已有数据则跳过，避免重复播种
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        // 种子用户：申请人（张三）、直属领导（李四）、部门经理（王五）
        var applicant = new User
        {
            UserName = "zhangsan",
            DisplayName = "张三",
            Email = "zhangsan@bjcx.com",
            EmployeeNo = "E001",
            Department = "研发部",
            PasswordHash = PasswordHasher.Hash(DefaultPassword),
            IsActive = true
        };

        var leader = new User
        {
            UserName = "lisi",
            DisplayName = "李四",
            Email = "lisi@bjcx.com",
            EmployeeNo = "E002",
            Department = "研发部",
            PasswordHash = PasswordHasher.Hash(DefaultPassword),
            IsActive = true
        };

        var manager = new User
        {
            UserName = "wangwu",
            DisplayName = "王五",
            Email = "wangwu@bjcx.com",
            EmployeeNo = "E003",
            Department = "研发部",
            PasswordHash = PasswordHasher.Hash(DefaultPassword),
            IsActive = true
        };

        dbContext.Users.AddRange(applicant, leader, manager);
        await dbContext.SaveChangesAsync();

        // 默认审批流程：直属领导 → 部门经理
        var workflow = new WorkflowDefinition
        {
            Name = "请假审批流程",
            Description = "默认流程：直属领导审批 → 部门经理审批",
            IsEnabled = true
        };
        workflow.Nodes.Add(new WorkflowNode { Order = 0, Name = "直属领导审批", ApproverId = leader.Id });
        workflow.Nodes.Add(new WorkflowNode { Order = 1, Name = "部门经理审批", ApproverId = manager.Id });

        dbContext.WorkflowDefinitions.Add(workflow);
        await dbContext.SaveChangesAsync();
    }
}
