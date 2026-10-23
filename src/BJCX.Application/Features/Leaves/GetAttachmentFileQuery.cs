using BJCX.Application.Abstractions;
using BJCX.Application.Exceptions;
using BJCX.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Leaves;

/// <summary>
/// 附件文件内容（下载/预览用）。
/// </summary>
public class AttachmentFileResult
{
    /// <summary>文件流。</summary>
    public Stream Stream { get; set; } = null!;

    /// <summary>原始文件名。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>内容类型（MIME）。</summary>
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>查询附件文件内容。</summary>
public class GetAttachmentFileQuery : IRequest<AttachmentFileResult>
{
    public long AttachmentId { get; set; }
}

/// <summary>附件文件查询处理器：查元数据 + 读文件流。</summary>
public class GetAttachmentFileQueryHandler : IRequestHandler<GetAttachmentFileQuery, AttachmentFileResult>
{
    private readonly IRepository<LeaveAttachment> _attachmentRepository;
    private readonly IFileStorageService _fileStorageService;

    public GetAttachmentFileQueryHandler(IRepository<LeaveAttachment> attachmentRepository, IFileStorageService fileStorageService)
    {
        _attachmentRepository = attachmentRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<AttachmentFileResult> Handle(GetAttachmentFileQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.QueryAsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken)
            ?? throw new NotFoundException("附件不存在");

        var stream = await _fileStorageService.GetAsync(attachment.StorageKey, cancellationToken)
            ?? throw new NotFoundException("附件文件不存在");

        return new AttachmentFileResult
        {
            Stream = stream,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType
        };
    }
}
