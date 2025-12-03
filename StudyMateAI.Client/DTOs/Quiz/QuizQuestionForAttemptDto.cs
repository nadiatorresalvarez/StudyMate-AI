namespace StudyMateAI.Client.DTOs.Quiz;

public class QuizQuestionForAttemptDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "MultipleChoice"; 
    public List<string> Options { get; set; } = new();
}