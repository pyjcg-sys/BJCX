namespace BJCX.Domain.Entities;

/// <summary>
/// 邮件消息：持久化模拟发送的邮件（不接真实 SMTP，仅存库供查看与审计）。
/// </summary>
public class EmailMessage : BaseEntity
{
    /// <summary>收件人用户 Id（用于按收件人隔离查询）。</summary>
    public long ToUserId { get; set; }

    /// <summary>收件人姓名（冗余存储，显示用，无需关联查询）。</summary>
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
