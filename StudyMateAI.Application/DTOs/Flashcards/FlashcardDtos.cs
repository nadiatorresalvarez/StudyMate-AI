namespace StudyMateAI.Application.DTOs.Flashcards;

public class FlashcardResponseDto
{
    public int Id { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public string Difficulty { get; set; } = null!;
    public int DocumentId { get; set; }
}

public class CreateFlashcardRequestDto
{
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public string Difficulty { get; set; } = "Medium";
}

public class UpdateFlashcardRequestDto
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? Difficulty { get; set; }
}

public class ReviewFlashcardRequestDto
{
    public int Quality { get; set; }
}
