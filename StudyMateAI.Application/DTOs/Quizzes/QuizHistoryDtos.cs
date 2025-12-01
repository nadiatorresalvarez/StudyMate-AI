namespace StudyMateAI.Application.DTOs.Quizzes;

public class QuizAttemptSummaryDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public int DocumentId { get; set; }
    public string QuizTitle { get; set; } = null!;
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public double ScorePercent { get; set; }
    public int? TimeSpentSeconds { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class WeakTopicDto
{
    public string Topic { get; set; } = null!;
    public int FailedCount { get; set; }
    public int TotalCount { get; set; }
    public double FailureRate { get; set; }
}

public class QuizHistoryResponseDto
{
    public IReadOnlyList<QuizAttemptSummaryDto> Attempts { get; set; } = Array.Empty<QuizAttemptSummaryDto>();
    public IReadOnlyList<WeakTopicDto> WeakTopics { get; set; } = Array.Empty<WeakTopicDto>();
}
