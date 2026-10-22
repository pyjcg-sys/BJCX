namespace BJCX.Application.DTOs;

/// <summary>
/// 附件元信息 DTO。
/// </summary>
public class AttachmentDto
{
    /// <summary>附件 Id。</summary>
    public long Id { get; set; }

    /// <summary>原始文件名。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>内容类型（MIME）。</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>文件大小（字节）。</summary>
    public long FileSize { get; set; }

    /// <summary>上传时间。</summary>
    public DateTime UploadedAt { get; set; }
}
