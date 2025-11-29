using Microsoft.Extensions.Configuration;
using StudyMateAI.Application.Common.Abstractions;
using System.IO;

namespace StudyMateAI.Infrastructure.Adapters.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(IConfiguration configuration)
    {
        // Default fallback
        _rootPath = configuration["Storage:RootPath"] ?? "wwwroot/uploads";
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, string contentType, string relativePath, CancellationToken ct)
    {
        var folder = Path.Combine(_rootPath, relativePath);
        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, fileName);

        // overwrite if exists
        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fileStream, ct);
        }

        // Return a relative URL-like path e.g. "/uploads/{relativePath}/{fileName}"
        var normalizedRelative = relativePath.Replace("\\", "/").Trim('/');
        var url = $"/uploads/{normalizedRelative}/{fileName}";

        return url;
    }
}
