using BJCX.Domain.Enums;

namespace BJCX.Application.DTOs;

/// <summary>
/// 审批操作记录 DTO。
/// </summary>
public class ApprovalRecordDto
{
    public long Id { get; set; }

    public int NodeOrder { get; set; }

    public string NodeName { get; set; } = string.Empty;

    public long ApproverId { get; set; }

    public string ApproverName { get; set; } = string.Empty;

    /// <summary>处理动作（Pending/Approved/Rejected）。</summary>
    public ApprovalAction Action { get; set; }

    public string? Comment { get; set; }

    public DateTime ActionTime { get; set; }
}
