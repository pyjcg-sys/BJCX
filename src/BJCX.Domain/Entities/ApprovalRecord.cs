using BJCX.Domain.Enums;

namespace BJCX.Domain.Entities;

/// <summary>
/// 审批操作记录：记录流程中每个节点的处理结果，构成完整的审批链路。
/// </summary>
public class ApprovalRecord : BaseEntity
{
    /// <summary>所属请假单据 Id。</summary>
    public long LeaveRequestId { get; set; }

    /// <summary>所属请假单据（导航）。</summary>
    public LeaveRequest LeaveRequest { get; set; } = null!;

    /// <summary>对应流程节点的序号。</summary>
    public int NodeOrder { get; set; }

    /// <summary>节点名称（快照，避免流程配置变更影响历史记录）。</summary>
    public string NodeName { get; set; } = null!;

    /// <summary>审批人 Id。</summary>
    public long ApproverId { get; set; }

    /// <summary>审批人姓名（快照）。</summary>
    public string ApproverName { get; set; } = null!;

    /// <summary>处理动作（待审批 / 同意 / 驳回）。</summary>
    public ApprovalAction Action { get; set; }

    /// <summary>审批意见（同意或驳回时的备注）。</summary>
    public string? Comment { get; set; }

    /// <summary>操作时间。</summary>
    public DateTime ActionTime { get; set; }
}
