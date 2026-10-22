using BJCX.Domain.Enums;

namespace BJCX.Application.DTOs;

/// <summary>
/// 请假单据基础信息 DTO（列表项 / 详情基类）。
/// </summary>
public class LeaveDto
{
    /// <summary>单据 Id。</summary>
    public long Id { get; set; }

    /// <summary>单据编号。</summary>
    public string RequestNo { get; set; } = string.Empty;

    /// <summary>申请人 Id。</summary>
    public long ApplicantId { get; set; }

    /// <summary>申请人姓名。</summary>
    public string ApplicantName { get; set; } = string.Empty;

    /// <summary>请假类型（Annual/Personal/Sick/Marriage/Other）。</summary>
    public LeaveType LeaveType { get; set; }

    /// <summary>开始日期。</summary>
    public DateTime StartDate { get; set; }

    /// <summary>结束日期。</summary>
    public DateTime EndDate { get; set; }

    /// <summary>请假天数。</summary>
    public int Days { get; set; }

    /// <summary>请假事由。</summary>
    public string? Reason { get; set; }

    /// <summary>单据状态（Pending/Approved/Rejected）。</summary>
    public LeaveStatus Status { get; set; }

    /// <summary>当前待审批节点序号（-1 表示流程已结束）。</summary>
    public int CurrentNodeOrder { get; set; }

    /// <summary>创建时间。</summary>
    public DateTime CreatedAt { get; set; }
}
