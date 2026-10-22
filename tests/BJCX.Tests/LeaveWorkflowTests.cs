using BJCX.Application.Abstractions;
using BJCX.Application.Exceptions;
using BJCX.Application.Features.Leaves;
using BJCX.Domain.Entities;
using BJCX.Domain.Enums;
using BJCX.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using Xunit;

namespace BJCX.Tests;

/// <summary>请假审批流程集成测试：覆盖提交、同意流转、驳回、详情、待办列表与权限校验。</summary>
public class LeaveWorkflowTests : TestBase
{
    [Fact]
    public async Task 提交申请_创建单据并通知直属领导()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");

        var id = await SubmitAsync(applicant);

        id.Should().BeGreaterThan(0);

        var leave = await DbContext.LeaveRequests.AsNoTracking()
            .Include(l => l.ApprovalRecords)
            .FirstAsync(l => l.Id == id);

        leave.Status.Should().Be(LeaveStatus.Pending);
        leave.CurrentNodeOrder.Should().Be(0);
        leave.ApplicantId.Should().Be(ApplicantId);
        leave.RequestNo.Should().StartWith("LX-");

        leave.ApprovalRecords.Should().ContainSingle();
        var record = leave.ApprovalRecords.First();
        record.ApproverId.Should().Be(LeaderId);
        record.Action.Should().Be(ApprovalAction.Pending);

        EmailServiceMock.Verify(e => e.SendAsync("lisi@bjcx.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task 直属领导同意_流转到部门经理()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");
        var id = await SubmitAsync(applicant);

        var leader = CreateCurrentUser(LeaderId, "lisi", "李四");
        await CreateApproveHandler(leader).Handle(new ApproveLeaveCommand { LeaveRequestId = id, Comment = "同意" }, CancellationToken.None);

        var leave = await DbContext.LeaveRequests.AsNoTracking()
            .Include(l => l.ApprovalRecords)
            .FirstAsync(l => l.Id == id);

        leave.Status.Should().Be(LeaveStatus.Pending);
        leave.CurrentNodeOrder.Should().Be(1);
        leave.ApprovalRecords.Should().HaveCount(2);
        leave.ApprovalRecords.Should().ContainSingle(r => r.NodeOrder == 0 && r.Action == ApprovalAction.Approved);
        leave.ApprovalRecords.Should().ContainSingle(r => r.NodeOrder == 1 && r.Action == ApprovalAction.Pending);

        EmailServiceMock.Verify(e => e.SendAsync("wangwu@bjcx.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task 部门经理同意_单据通过并通知申请人()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");
        var id = await SubmitAsync(applicant);

        await CreateApproveHandler(CreateCurrentUser(LeaderId, "lisi", "李四"))
            .Handle(new ApproveLeaveCommand { LeaveRequestId = id, Comment = "同意" }, CancellationToken.None);

        await CreateApproveHandler(CreateCurrentUser(ManagerId, "wangwu", "王五"))
            .Handle(new ApproveLeaveCommand { LeaveRequestId = id, Comment = "同意" }, CancellationToken.None);

        var leave = await DbContext.LeaveRequests.AsNoTracking().FirstAsync(l => l.Id == id);
        leave.Status.Should().Be(LeaveStatus.Approved);
        leave.CurrentNodeOrder.Should().Be(-1);

        EmailServiceMock.Verify(e => e.SendAsync("zhangsan@bjcx.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task 部门经理越权审批第一节点_抛业务异常()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");
        var id = await SubmitAsync(applicant);

        var manager = CreateCurrentUser(ManagerId, "wangwu", "王五");
        var handler = CreateApproveHandler(manager);

        Func<Task> act = () => handler.Handle(new ApproveLeaveCommand { LeaveRequestId = id, Comment = "同意" }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>().WithMessage("您不是当前节点的审批人，无权限操作");
    }

    [Fact]
    public async Task 重复审批同一节点_抛业务异常()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");
        var id = await SubmitAsync(applicant);

        var leader = CreateCurrentUser(LeaderId, "lisi", "李四");
        await CreateApproveHandler(leader).Handle(new ApproveLeaveCommand { LeaveRequestId = id, Comment = "同意" }, CancellationToken.None);

        // 李四已审批完当前节点，再次审批应失败
        Func<Task> act = () => CreateApproveHandler(leader).Handle(new ApproveLeaveCommand { LeaveRequestId = id, Comment = "重复审批" }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task 直属领导驳回_单据驳回并通知申请人()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");
        var id = await SubmitAsync(applicant);

        var leader = CreateCurrentUser(LeaderId, "lisi", "李四");
        await CreateRejectHandler(leader).Handle(new RejectLeaveCommand { LeaveRequestId = id, Comment = "不同意，请补充材料" }, CancellationToken.None);

        var leave = await DbContext.LeaveRequests.AsNoTracking().FirstAsync(l => l.Id == id);
        leave.Status.Should().Be(LeaveStatus.Rejected);
        leave.CurrentNodeOrder.Should().Be(-1);

        EmailServiceMock.Verify(e => e.SendAsync("zhangsan@bjcx.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task 查询详情_返回附件与审批记录()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");
        var id = await SubmitAsync(applicant);

        DbContext.LeaveAttachments.Add(new LeaveAttachment
        {
            LeaveRequestId = id,
            FileName = "请假单.pdf",
            StorageKey = "key-1",
            ContentType = "application/pdf",
            FileSize = 1024,
            UploadedAt = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();

        var handler = new GetLeaveDetailQueryHandler(new Repository<LeaveRequest>(DbContext), CacheService, Mapper);
        var dto = await handler.Handle(new GetLeaveDetailQuery { LeaveRequestId = id }, CancellationToken.None);

        dto.Id.Should().Be(id);
        dto.RequestNo.Should().StartWith("LX-");
        dto.Attachments.Should().ContainSingle(a => a.FileName == "请假单.pdf");
        dto.ApprovalRecords.Should().HaveCount(1);
    }

    [Fact]
    public async Task 待办列表_按当前用户过滤()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");
        await SubmitAsync(applicant);

        var leader = CreateCurrentUser(LeaderId, "lisi", "李四");
        var handler = CreatePendingListHandler(leader);

        var result = await handler.Handle(new GetMyPendingListQuery { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(l => l.ApplicantName == "张三");
    }

    [Fact]
    public async Task 待办列表_部门经理在首节点无待办()
    {
        var applicant = CreateCurrentUser(ApplicantId, "zhangsan", "张三");
        await SubmitAsync(applicant);

        var manager = CreateCurrentUser(ManagerId, "wangwu", "王五");
        var handler = CreatePendingListHandler(manager);

        var result = await handler.Handle(new GetMyPendingListQuery { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(0);
    }

    private async Task<long> SubmitAsync(ICurrentUserService currentUser)
        => await CreateSubmitHandler(currentUser).Handle(new SubmitLeaveCommand
        {
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(2),
            Reason = "年假"
        }, CancellationToken.None);

    private SubmitLeaveCommandHandler CreateSubmitHandler(ICurrentUserService currentUser) => new(
        new Repository<User>(DbContext),
        new Repository<WorkflowDefinition>(DbContext),
        new Repository<LeaveRequest>(DbContext),
        new Repository<LeaveAttachment>(DbContext),
        new Repository<ApprovalRecord>(DbContext),
        currentUser,
        EmailServiceMock.Object,
        CacheService,
        Log.Logger);

    private ApproveLeaveCommandHandler CreateApproveHandler(ICurrentUserService currentUser) => new(
        new Repository<LeaveRequest>(DbContext),
        new Repository<ApprovalRecord>(DbContext),
        currentUser,
        EmailServiceMock.Object,
        CacheService,
        Log.Logger);

    private RejectLeaveCommandHandler CreateRejectHandler(ICurrentUserService currentUser) => new(
        new Repository<LeaveRequest>(DbContext),
        currentUser,
        EmailServiceMock.Object,
        CacheService,
        Log.Logger);

    private GetMyPendingListQueryHandler CreatePendingListHandler(ICurrentUserService currentUser) => new(
        new Repository<ApprovalRecord>(DbContext),
        new Repository<LeaveRequest>(DbContext),
        currentUser,
        CacheService,
        Mapper);
}
