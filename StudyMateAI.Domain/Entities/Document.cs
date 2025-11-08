using System;
using System.Collections.Generic;

namespace StudyMateAI.Domain.Entities;

public partial class Document
{
    public int Id { get; set; }

    public string FileName { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    /// <summary>
    /// PDF, DOCX, PPTX, TXT
    /// </summary>
    public string FileType { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? ExtractedText { get; set; }

    public int? FileSizeKb { get; set; }

    public int? PageCount { get; set; }

    public string? Language { get; set; }

    /// <summary>
    /// Pending, Completed, Failed
    /// </summary>
    public string? ProcessingStatus { get; set; }

    public string? ProcessingError { get; set; }

    public int SubjectId { get; set; }

    public DateTime? UploadedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual ICollection<Conceptmap> Conceptmaps { get; set; } = new List<Conceptmap>();

    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();

    public virtual ICollection<Mindmap> Mindmaps { get; set; } = new List<Mindmap>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual ICollection<Studysession> Studysessions { get; set; } = new List<Studysession>();

    public virtual Subject Subject { get; set; } = null!;

    public virtual ICollection<Summary> Summaries { get; set; } = new List<Summary>();
}
