namespace BJCX.Application.DTOs;

/// <summary>
/// 附件元信息 DTO。
/// </summary>
public class AttachmentDto
{
    public long Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }
}
