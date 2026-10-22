using BJCX.Application.DTOs;
using BJCX.Application.Features.Workflows;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BJCX.Api.Controllers;

/// <summary>
/// 审批流程配置控制器（简易配置，不引入重型工作流框架）。
/// </summary>
[ApiController]
[Route("api/workflows")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkflowController(IMediator mediator) => _mediator = mediator;

    /// <summary>创建审批流程（含节点与审批人）。</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    /// <summary>查询审批流程列表。</summary>
    [HttpGet]
    public async Task<ActionResult<List<WorkflowListItemDto>>> GetList()
    {
        return Ok(await _mediator.Send(new GetWorkflowsQuery()));
    }

    /// <summary>查询审批流程详情（含节点与审批人）。</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<WorkflowDefinitionDto>> GetDetail(long id)
    {
        return Ok(await _mediator.Send(new GetWorkflowDetailQuery { WorkflowDefinitionId = id }));
    }
}
