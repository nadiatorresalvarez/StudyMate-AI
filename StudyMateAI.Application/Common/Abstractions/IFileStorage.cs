using System.IO;

namespace StudyMateAI.Application.Common.Abstractions;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, string relativePath, CancellationToken ct);
    Task DeleteAsync(string fileUrlOrPath, CancellationToken ct);
}
