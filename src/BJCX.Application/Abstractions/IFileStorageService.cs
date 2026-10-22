namespace BJCX.Application.Abstractions;

/// <summary>
/// 已存储文件的元信息。
/// </summary>
/// <param name="StorageKey">存储 Key（文件存储服务内唯一标识）。</param>
/// <param name="FileName">原始文件名。</param>
/// <param name="ContentType">内容类型。</param>
/// <param name="FileSize">文件大小（字节）。</param>
public record StoredFile(string StorageKey, string FileName, string ContentType, long FileSize);

/// <summary>
/// 文件存储服务抽象。内部抽象即可，不接入真实第三方（OSS/S3 等）。
/// 当前由 LocalFileStorageService 落盘实现。
/// </summary>
public interface IFileStorageService
{
    /// <summary>保存文件流并返回元信息。</summary>
    Task<StoredFile> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>按 Key 读取文件流，不存在时返回 null。</summary>
    Task<Stream?> GetAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>按 Key 删除文件。</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
