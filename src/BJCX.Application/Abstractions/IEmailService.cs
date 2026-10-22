namespace BJCX.Application.Abstractions;

/// <summary>
/// 邮件服务抽象。内部抽象即可，不接入真实第三方（SMTP/SendGrid 等）。
/// 当前由 LogEmailService 模拟实现：仅记录结构化日志并写入内存发件箱。
/// </summary>
public interface IEmailService
{
    /// <summary>发送邮件。</summary>
    /// <param name="to">收件人邮箱。</param>
    /// <param name="subject">主题。</param>
    /// <param name="body">正文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
