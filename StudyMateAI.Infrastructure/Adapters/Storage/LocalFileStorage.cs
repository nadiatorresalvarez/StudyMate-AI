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

    public Task DeleteAsync(string fileUrlOrPath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(fileUrlOrPath))
        {
            return Task.CompletedTask;
        }

        // Si viene como URL tipo "/uploads/{relative}/{fileName}", la convertimos a ruta física
        var path = fileUrlOrPath.Trim();
        if (path.StartsWith("/"))
        {
            path = path.TrimStart('/');
        }

        if (path.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring("uploads/".Length);
        }

        // Ahora path es relativo a _rootPath (por ejemplo "userId/subjectId/file.ext")
        var fullPath = Path.Combine(_rootPath, path.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
