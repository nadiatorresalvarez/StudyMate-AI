namespace StudyMateAI.Client.DTOs.Quiz;

public class QuizAttemptResultDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public double ScorePercent { get; set; }
    public List<QuestionResultDto> Questions { get; set; } = new();
}