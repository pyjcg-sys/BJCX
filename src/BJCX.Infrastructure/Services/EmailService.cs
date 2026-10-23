using BJCX.Application.Abstractions;
using BJCX.Domain.Entities;
using Serilog;

namespace BJCX.Infrastructure.Services;

/// <summary>
/// 邮件服务：不接入真实 SMTP，将邮件（含收件人信息）持久化到数据库并记录结构化日志。
/// </summary>
public class EmailService : IEmailService
{
    private readonly IRepository<EmailMessage> _emailRepository;
    private readonly ILogger _logger;

    public EmailService(IRepository<EmailMessage> emailRepository, ILogger logger)
    {
        _emailRepository = emailRepository;
        _logger = logger;
    }

    public async Task SendAsync(EmailRecipient recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        await _emailRepository.AddAsync(new EmailMessage
        {
            ToUserId = recipient.UserId,
            ToUserName = recipient.DisplayName,
            To = recipient.Email,
            Subject = subject,
            Body = body,
            SentAt = DateTime.UtcNow
        }, cancellationToken);
        await _emailRepository.SaveChangesAsync(cancellationToken);

        _logger.Information("发送邮件：To={To}, Subject={Subject}", recipient.Email, subject);
    }
}
