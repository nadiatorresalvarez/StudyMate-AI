using System;

namespace StudyMateAI.Application.DTOs.Document
{
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string OriginalFileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string? ExtractedText { get; set; }
        public int? FileSizeKb { get; set; }
        public int? PageCount { get; set; }
        public string? Language { get; set; }
        public string? ProcessingStatus { get; set; }
        public string? ProcessingError { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public DateTime? UploadedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        
        // Contadores de recursos generados
        public int FlashcardCount { get; set; }
        public int QuizCount { get; set; }
        public int SummaryCount { get; set; }
        public int MindmapCount { get; set; }
        public int ConceptmapCount { get; set; }
    }
}

