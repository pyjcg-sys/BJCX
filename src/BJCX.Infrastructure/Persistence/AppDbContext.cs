using BJCX.Domain.Entities;
using BJCX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Infrastructure.Persistence;

/// <summary>
/// EF Core 数据库上下文，负责实体映射与关系/索引配置。
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowNode> WorkflowNodes => Set<WorkflowNode>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveAttachment> LeaveAttachments => Set<LeaveAttachment>();
    public DbSet<ApprovalRecord> ApprovalRecords => Set<ApprovalRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 用户：登录名、工号唯一
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.EmployeeNo).IsUnique();
        });

        // 请假单据：单据编号唯一；枚举存 int；外键限制删除
        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasIndex(x => x.RequestNo).IsUnique();
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.LeaveType).HasConversion<int>();
            e.HasOne(x => x.Applicant).WithMany().HasForeignKey(x => x.ApplicantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
        });

        // 流程节点：随流程定义级联删除；审批人限制删除
        modelBuilder.Entity<WorkflowNode>(e =>
        {
            e.HasOne(x => x.WorkflowDefinition).WithMany(w => w.Nodes).HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Approver).WithMany().HasForeignKey(x => x.ApproverId).OnDelete(DeleteBehavior.Restrict);
        });

        // 附件：随单据级联删除
        modelBuilder.Entity<LeaveAttachment>(e =>
        {
            e.HasOne(x => x.LeaveRequest).WithMany(l => l.Attachments).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);
        });

        // 审批记录：随单据级联删除；枚举存 int
        modelBuilder.Entity<ApprovalRecord>(e =>
        {
            e.Property(x => x.Action).HasConversion<int>();
            e.HasOne(x => x.LeaveRequest).WithMany(l => l.ApprovalRecords).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
