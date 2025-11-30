namespace StudyMateAI.Application.DTOs.Flashcards;

public class NextStudyFlashcardDto
{
    public int FlashcardId { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public string Difficulty { get; set; } = null!;
    public DateTime? NextReviewDate { get; set; }
    public int? ReviewCount { get; set; }
    public int? IntervalDays { get; set; }
    public double EaseFactor { get; set; }
}
