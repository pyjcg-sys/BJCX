using BJCX.Application.DTOs;
using BJCX.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BJCX.Api.Controllers;

/// <summary>
/// 认证控制器：登录签发 JWT。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>登录：校验账号密码，返回 JWT 访问令牌。</summary>
    /// <param name="command">登录命令（用户名、密码）。</param>
    /// <returns>登录结果（JWT 令牌与用户信息）。</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
