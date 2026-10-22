using System.Text.Json;
using BJCX.Application.Exceptions;
using FluentValidation;

namespace BJCX.Api.Middleware;

/// <summary>
/// 全局异常处理中间件：将校验异常、业务异常、未捕获异常转换为统一 JSON 错误结构。
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            // 入参校验失败 → 400
            ValidationException validationEx => (StatusCodes.Status400BadRequest,
                string.Join("；", validationEx.Errors.Select(e => e.ErrorMessage))),
            // 资源不存在 → 404
            NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
            // 业务规则失败 → 400
            BusinessException => (StatusCodes.Status400BadRequest, ex.Message),
            // 其它 → 500
            _ => (StatusCodes.Status500InternalServerError, "服务器内部错误，请稍后重试")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(ex, "未处理异常：{Message}", ex.Message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            statusCode,
            message
        }));
    }
}
