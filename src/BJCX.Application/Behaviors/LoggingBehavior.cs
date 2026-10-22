using System.Diagnostics;
using BJCX.Application.Abstractions;
using MediatR;
using Serilog;

namespace BJCX.Application.Behaviors;

/// <summary>
/// 日志管道：以结构化日志记录每次请求的操作人、请求名、耗时与异常。
/// 单据编号等业务标识由各 Handler 内部用 ForContext 补充。
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(ILogger logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger
                .ForContext("Operator", _currentUserService.UserName)
                .ForContext("OperatorId", _currentUserService.UserId)
                .ForContext("RequestName", requestName)
                .ForContext("ElapsedMs", stopwatch.ElapsedMilliseconds)
                .Information("处理请求 {RequestName} 完成，耗时 {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger
                .ForContext("Operator", _currentUserService.UserName)
                .ForContext("OperatorId", _currentUserService.UserId)
                .ForContext("RequestName", requestName)
                .ForContext("ElapsedMs", stopwatch.ElapsedMilliseconds)
                .ForContext("Exception", ex, destructureObjects: true)
                .Error(ex, "处理请求 {RequestName} 失败", requestName);

            throw;
        }
    }
}
