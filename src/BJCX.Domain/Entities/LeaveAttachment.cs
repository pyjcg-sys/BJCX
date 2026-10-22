namespace BJCX.Domain.Entities;

/// <summary>
/// 请假附件元数据（文件实体存储在文件存储服务中，此处仅保存元信息）。
/// </summary>
public class LeaveAttachment : BaseEntity
{
    /// <summary>所属请假单据 Id。</summary>
    public long LeaveRequestId { get; set; }

    /// <summary>所属请假单据（导航）。</summary>
    public LeaveRequest LeaveRequest { get; set; } = null!;

    /// <summary>原始文件名。</summary>
    public string FileName { get; set; } = null!;

    /// <summary>存储 Key（文件存储服务内的唯一标识）。</summary>
    public string StorageKey { get; set; } = null!;

    /// <summary>内容类型（MIME）。</summary>
    public string ContentType { get; set; } = null!;

    /// <summary>文件大小（字节）。</summary>
    public long FileSize { get; set; }

    /// <summary>上传时间。</summary>
    public DateTime UploadedAt { get; set; }
}
