namespace BJCX.Api.Contracts;

/// <summary>审批请求体（同意 / 驳回共用）。</summary>
public class ApprovalRequest
{
    /// <summary>审批 / 驳回意见。</summary>
    public string Comment { get; set; } = string.Empty;
}
