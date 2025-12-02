namespace StudyMateAI.Client.DTOs.Document;

public class DocumentResponseDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int? FileSizeKb { get; set; }
    public int? PageCount { get; set; }
        
    // Estado del procesamiento (Pending, Completed, Failed)
    public string ProcessingStatus { get; set; } = string.Empty;
    public string? ProcessingError { get; set; }
        
    public int SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public DateTime CreatedAt { get; set; }

    // Contadores para saber si ya tiene material generado
    public int FlashcardCount { get; set; }
    public int QuizCount { get; set; }
    public int SummaryCount { get; set; }
}