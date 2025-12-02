namespace StudyMateAI.Client.DTOs.Document;

public class DocumentResponseDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty; // Inicializado
    public string OriginalFileName { get; set; } = string.Empty; // Inicializado
    public string FileType { get; set; } = string.Empty; // Inicializado
    public int? FileSizeKb { get; set; }
    public int? PageCount { get; set; }
        
    public string ProcessingStatus { get; set; } = string.Empty;
    public string? ProcessingError { get; set; }
        
    public int SubjectId { get; set; }
    public string? SubjectName { get; set; } // Puede venir nulo
    public DateTime CreatedAt { get; set; }

    public int FlashcardCount { get; set; }
    public int QuizCount { get; set; }
    public int SummaryCount { get; set; }
}