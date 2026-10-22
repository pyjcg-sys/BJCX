using System.Security.Claims;
using BJCX.Application.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BJCX.Api.Filters;

/// <summary>
/// 身份解析 Filter：在 Action 执行前从 JWT（HttpContext.User）解析当前用户，
/// 写入 ICurrentUserService，Controller 与 Handler 不再重复解析 Token。
/// </summary>
public class CurrentUserFilter : IAsyncActionFilter
{
    private readonly ICurrentUserService _currentUserService;

    public CurrentUserFilter(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = long.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (long?)null;
            var userName = user.FindFirstValue(ClaimTypes.Name);
            var displayName = user.FindFirstValue("displayName");
            var email = user.FindFirstValue(ClaimTypes.Email);

            _currentUserService.SetUser(userId, userName, displayName, email);
        }
        else
        {
            _currentUserService.SetUser(null, null, null, null);
        }

        await next();
    }
}
