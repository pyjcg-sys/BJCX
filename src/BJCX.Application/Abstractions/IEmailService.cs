namespace BJCX.Application.Abstractions;

/// <summary>
/// 邮件收件人信息（含用户 Id，用于「按收件人隔离」）。
/// </summary>
/// <param name="UserId">收件人用户 Id。</param>
/// <param name="DisplayName">收件人姓名。</param>
/// <param name="Email">收件人邮箱。</param>
public record EmailRecipient(long UserId, string DisplayName, string Email);

/// <summary>
/// 邮件服务抽象。内部抽象即可，不接入真实第三方（SMTP/SendGrid 等）。
/// 当前由 EmailService 实现：将邮件持久化到数据库，并按收件人隔离。
/// </summary>
public interface IEmailService
{
    /// <summary>发送邮件。</summary>
    /// <param name="recipient">收件人信息（含用户 Id、姓名、邮箱）。</param>
    /// <param name="subject">主题。</param>
    /// <param name="body">正文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task SendAsync(EmailRecipient recipient, string subject, string body, CancellationToken cancellationToken = default);
}
