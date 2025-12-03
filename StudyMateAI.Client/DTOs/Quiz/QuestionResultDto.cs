namespace StudyMateAI.Client.DTOs.Quiz;

public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? SelectedOption { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public List<string> Options { get; set; } = new();
}