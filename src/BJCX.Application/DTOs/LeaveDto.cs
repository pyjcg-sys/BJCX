using BJCX.Domain.Enums;

namespace BJCX.Application.DTOs;

/// <summary>
/// 请假单据基础信息 DTO（列表项 / 详情基类）。
/// </summary>
public class LeaveDto
{
    public long Id { get; set; }

    /// <summary>单据编号。</summary>
    public string RequestNo { get; set; } = string.Empty;

    public long ApplicantId { get; set; }

    public string ApplicantName { get; set; } = string.Empty;

    public LeaveType LeaveType { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int Days { get; set; }

    public string? Reason { get; set; }

    /// <summary>单据状态。</summary>
    public LeaveStatus Status { get; set; }

    /// <summary>当前待审批节点序号。</summary>
    public int CurrentNodeOrder { get; set; }

    public DateTime CreatedAt { get; set; }
}
