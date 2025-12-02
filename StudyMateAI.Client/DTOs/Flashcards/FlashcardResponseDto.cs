namespace StudyMateAI.Client.DTOs.Flashcards;

public class FlashcardResponseDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    public int DocumentId { get; set; }
}