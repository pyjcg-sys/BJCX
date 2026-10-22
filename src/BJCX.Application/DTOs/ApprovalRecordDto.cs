using BJCX.Domain.Enums;

namespace BJCX.Application.DTOs;

/// <summary>
/// 审批操作记录 DTO。
/// </summary>
public class ApprovalRecordDto
{
    /// <summary>记录 Id。</summary>
    public long Id { get; set; }

    /// <summary>对应流程节点序号。</summary>
    public int NodeOrder { get; set; }

    /// <summary>节点名称。</summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>审批人 Id。</summary>
    public long ApproverId { get; set; }

    /// <summary>审批人姓名。</summary>
    public string ApproverName { get; set; } = string.Empty;

    /// <summary>处理动作（Pending/Approved/Rejected）。</summary>
    public ApprovalAction Action { get; set; }

    /// <summary>审批意见。</summary>
    public string? Comment { get; set; }

    /// <summary>操作时间。</summary>
    public DateTime ActionTime { get; set; }
}
