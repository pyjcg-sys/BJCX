namespace BJCX.Application.DTOs;

/// <summary>
/// 邮件消息 DTO。
/// </summary>
public class EmailMessageDto
{
    /// <summary>邮件 Id。</summary>
    public long Id { get; set; }

    /// <summary>收件人。</summary>
    public string To { get; set; } = string.Empty;

    /// <summary>主题。</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>正文。</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>发送时间。</summary>
    public DateTime SentAt { get; set; }
}
