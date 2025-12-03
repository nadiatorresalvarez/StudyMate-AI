namespace StudyMateAI.Client.DTOs.Quiz;

public class QuizForAttemptDto
{
    public int QuizId { get; set; }
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<QuizQuestionForAttemptDto> Questions { get; set; } = new();
}