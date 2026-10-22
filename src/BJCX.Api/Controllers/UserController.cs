using BJCX.Application.DTOs;
using BJCX.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BJCX.Api.Controllers;

/// <summary>
/// 用户控制器：提供用户基础数据（流程配置选择审批人用）。
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator) => _mediator = mediator;

    /// <summary>查询启用的用户列表。</summary>
    /// <returns>用户列表。</returns>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetList()
    {
        return Ok(await _mediator.Send(new GetUsersQuery()));
    }
}
