namespace StudyMateAI.Application.DTOs.Quizzes;

public class SubmittedAnswerDto
{
    public int QuestionId { get; set; }
    public string? SelectedOption { get; set; }
    public string? AnswerText { get; set; }
}

public class SubmitQuizAttemptRequestDto
{
    public List<SubmittedAnswerDto> Answers { get; set; } = new();
}

public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = null!;
    public string? SelectedOption { get; set; }
    public string? SelectedAnswerText { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
}

public class QuizAttemptResultDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public int DocumentId { get; set; }
    public string QuizTitle { get; set; } = null!;
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public double ScorePercent { get; set; }
    public int? TimeSpentSeconds { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public IReadOnlyList<QuestionResultDto> Questions { get; set; } = Array.Empty<QuestionResultDto>();
}
