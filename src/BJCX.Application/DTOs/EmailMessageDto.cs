namespace BJCX.Application.DTOs;

/// <summary>
/// 邮件消息 DTO。
/// </summary>
public class EmailMessageDto
{
    /// <summary>邮件 Id。</summary>
    public long Id { get; set; }

    /// <summary>收件人用户 Id。</summary>
    public long ToUserId { get; set; }

    /// <summary>收件人姓名。</summary>
    public string ToUserName { get; set; } = string.Empty;

    /// <summary>收件人邮箱。</summary>
    public string To { get; set; } = string.Empty;

    /// <summary>主题。</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>正文。</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>发送时间。</summary>
    public DateTime SentAt { get; set; }
}
