using BJCX.Api.Contracts;
using BJCX.Application.Abstractions;
using BJCX.Application.DTOs;
using BJCX.Application.Features.Leaves;
using BJCX.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BJCX.Api.Controllers;

/// <summary>
/// 请假审批控制器：仅做请求接收与调用 MediatR，不包含业务逻辑。
/// </summary>
[ApiController]
[Route("api/leaves")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorageService;

    public LeaveController(IMediator mediator, IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
    }

    /// <summary>提交请假申请，支持一并上传多个附件（multipart/form-data）。</summary>
    [HttpPost("submit")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Submit(
        [FromForm] LeaveType leaveType,
        [FromForm] DateTime startDate,
        [FromForm] DateTime endDate,
        [FromForm] string? reason,
        [FromForm] List<IFormFile>? attachments)
    {
        var command = new SubmitLeaveCommand
        {
            LeaveType = leaveType,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason
        };

        // 先将上传文件落盘，得到存储 Key，再组装命令交给应用层
        if (attachments is not null)
        {
            foreach (var file in attachments)
            {
                var stored = await _fileStorageService.SaveAsync(file.OpenReadStream(), file.FileName, file.ContentType ?? "application/octet-stream");
                command.Attachments.Add(new AttachmentUploadDto
                {
                    FileName = stored.FileName,
                    StorageKey = stored.StorageKey,
                    ContentType = stored.ContentType,
                    FileSize = stored.FileSize
                });
            }
        }

        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    /// <summary>审批同意：按串行规则流转到下一审批节点。</summary>
    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> Approve(long id, [FromBody] ApprovalRequest request)
    {
        await _mediator.Send(new ApproveLeaveCommand { LeaveRequestId = id, Comment = request.Comment });
        return Ok(new { message = "审批通过" });
    }

    /// <summary>审批驳回：驳回单据并通知申请人。</summary>
    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> Reject(long id, [FromBody] ApprovalRequest request)
    {
        await _mediator.Send(new RejectLeaveCommand { LeaveRequestId = id, Comment = request.Comment });
        return Ok(new { message = "已驳回" });
    }

    /// <summary>查询请假单据详情（基础信息 + 附件 + 审批记录，走 Redis 缓存）。</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<LeaveDetailDto>> GetDetail(long id)
    {
        return Ok(await _mediator.Send(new GetLeaveDetailQuery { LeaveRequestId = id }));
    }

    /// <summary>查询我的待审批单据分页列表。</summary>
    [HttpGet("pending")]
    public async Task<ActionResult<PagedResult<LeaveDto>>> GetPending([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _mediator.Send(new GetMyPendingListQuery { PageIndex = pageIndex, PageSize = pageSize }));
    }
}
