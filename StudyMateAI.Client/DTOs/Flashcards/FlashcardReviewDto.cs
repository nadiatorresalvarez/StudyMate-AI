namespace StudyMateAI.Client.DTOs.Flashcards;

public class FlashcardReviewDto
{
    public int Id { get; set; }
    public int FlashcardId { get; set; }
    public int Quality { get; set; }
    public DateTime ReviewedAt { get; set; }
}
