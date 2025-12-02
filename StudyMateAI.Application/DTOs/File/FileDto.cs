namespace StudyMateAI.Application.DTOs.File;

/// <summary>
/// DTO para transportar archivos binarios entre capas
/// </summary>
public class FileDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

