using BJCX.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace BJCX.Infrastructure.Services;

/// <summary>
/// 本地文件存储服务：文件落盘到本地目录（不接入真实第三方对象存储）。
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:RootPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "attachments");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredFile> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        // 使用 GUID 前缀保证文件名唯一，避免重名覆盖
        var storageKey = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_rootPath, storageKey);

        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return new StoredFile(storageKey, fileName, contentType, fileStream.Length);
    }

    public Task<Stream?> GetAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storageKey);
        return Task.FromResult(File.Exists(fullPath) ? (Stream?)File.OpenRead(fullPath) : null);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
