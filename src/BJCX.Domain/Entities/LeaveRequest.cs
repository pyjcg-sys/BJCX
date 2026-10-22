using BJCX.Domain.Enums;

namespace BJCX.Domain.Entities;

/// <summary>
/// 请假单据：一次请假申请的聚合根，承载流程状态。
/// </summary>
public class LeaveRequest : BaseEntity
{
    /// <summary>单据编号，规则 LX-yyyyMMdd-6位序号。</summary>
    public string RequestNo { get; set; } = null!;

    /// <summary>申请人 Id。</summary>
    public long ApplicantId { get; set; }

    /// <summary>申请人（导航）。</summary>
    public User Applicant { get; set; } = null!;

    /// <summary>申请人姓名（冗余存储，避免查询时连表）。</summary>
    public string ApplicantName { get; set; } = null!;

    /// <summary>请假类型。</summary>
    public LeaveType LeaveType { get; set; }

    /// <summary>开始日期。</summary>
    public DateTime StartDate { get; set; }

    /// <summary>结束日期。</summary>
    public DateTime EndDate { get; set; }

    /// <summary>请假天数。</summary>
    public int Days { get; set; }

    /// <summary>请假事由。</summary>
    public string? Reason { get; set; }

    /// <summary>所采用的审批流程定义 Id。</summary>
    public long WorkflowDefinitionId { get; set; }

    /// <summary>所采用的审批流程定义（导航）。</summary>
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    /// <summary>当前待审批的节点序号（-1 表示流程已结束）。</summary>
    public int CurrentNodeOrder { get; set; }

    /// <summary>单据状态。</summary>
    public LeaveStatus Status { get; set; }

    /// <summary>创建时间。</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>最后更新时间。</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>附件集合。</summary>
    public ICollection<LeaveAttachment> Attachments { get; set; } = new List<LeaveAttachment>();

    /// <summary>审批操作记录集合（按 NodeOrder 排序）。</summary>
    public ICollection<ApprovalRecord> ApprovalRecords { get; set; } = new List<ApprovalRecord>();
}
