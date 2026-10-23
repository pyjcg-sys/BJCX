using BJCX.Application.DTOs;
using BJCX.Application.Features.Emails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BJCX.Api.Controllers;

/// <summary>
/// 邮件控制器：查询持久化的邮件记录（模拟邮件，未真实投递）。
/// </summary>
[ApiController]
[Route("api/emails")]
[Authorize]
public class EmailController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmailController(IMediator mediator) => _mediator = mediator;

    /// <summary>分页查询邮件记录。</summary>
    /// <param name="pageIndex">页码（从 1 开始）。</param>
    /// <param name="pageSize">每页条数。</param>
    /// <returns>分页结果。</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmailMessageDto>>> GetList([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _mediator.Send(new GetEmailsQuery { PageIndex = pageIndex, PageSize = pageSize }));
    }
}
