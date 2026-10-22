using System.Collections.Concurrent;
using BJCX.Application.Abstractions;
using Serilog;

namespace BJCX.Infrastructure.Services;

/// <summary>邮件消息（内存发件箱条目）。</summary>
public record EmailMessage(string To, string Subject, string Body, DateTimeOffset SentAt);

/// <summary>
/// 模拟邮件服务：不接入真实 SMTP，仅记录结构化日志并写入内存发件箱，供演示与测试查看。
/// </summary>
public class LogEmailService : IEmailService
{
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<EmailMessage> _outbox = new();

    public LogEmailService(ILogger logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _outbox.Enqueue(new EmailMessage(to, subject, body, DateTimeOffset.UtcNow));
        _logger.Information("模拟发送邮件：To={To}, Subject={Subject}, Body={Body}", to, subject, body);
        return Task.CompletedTask;
    }

    /// <summary>内存发件箱快照（仅演示/测试用）。</summary>
    public IReadOnlyList<EmailMessage> Outbox => _outbox.ToList();
}
