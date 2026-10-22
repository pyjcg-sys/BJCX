namespace BJCX.Application.DTOs;

/// <summary>
/// 请假单据详情 DTO：基础信息 + 全部附件 + 完整审批操作记录。
/// </summary>
public class LeaveDetailDto : LeaveDto
{
    /// <summary>全部附件。</summary>
    public List<AttachmentDto> Attachments { get; set; } = new();

    /// <summary>完整审批操作记录（按节点顺序）。</summary>
    public List<ApprovalRecordDto> ApprovalRecords { get; set; } = new();
}
