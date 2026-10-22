using BJCX.Application.Features.Leaves;
using BJCX.Application.Features.Workflows;
using BJCX.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace BJCX.Tests;

/// <summary>FluentValidation 校验器单元测试。</summary>
public class ValidatorTests
{
    [Fact]
    public void 提交申请_结束日期早于开始日期_校验失败()
    {
        var validator = new SubmitLeaveCommandValidator();
        var command = new SubmitLeaveCommand
        {
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(-1)
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "结束日期不能早于开始日期");
    }

    [Fact]
    public void 提交申请_其他类型未填写事由_校验失败()
    {
        var validator = new SubmitLeaveCommandValidator();
        var command = new SubmitLeaveCommand
        {
            LeaveType = LeaveType.Other,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1),
            Reason = null
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void 提交申请_合法输入_校验通过()
    {
        var validator = new SubmitLeaveCommandValidator();
        var command = new SubmitLeaveCommand
        {
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(2),
            Reason = "年假"
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void 审批同意_无审批意见_校验通过()
    {
        var validator = new ApproveLeaveCommandValidator();
        var command = new ApproveLeaveCommand { LeaveRequestId = 1, Comment = string.Empty };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void 审批驳回_未填写驳回意见_校验失败()
    {
        var validator = new RejectLeaveCommandValidator();
        var command = new RejectLeaveCommand { LeaveRequestId = 1, Comment = string.Empty };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "驳回时必须填写驳回意见");
    }

    [Fact]
    public void 待办列表_页码非法_校验失败()
    {
        var validator = new GetMyPendingListQueryValidator();
        var query = new GetMyPendingListQuery { PageIndex = 0, PageSize = 10 };

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void 创建流程_未配置节点_校验失败()
    {
        var validator = new CreateWorkflowCommandValidator();
        var command = new CreateWorkflowCommand { Name = "测试流程", Nodes = new() };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
