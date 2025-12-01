namespace StudyMateAI.Application.DTOs.Quizzes;

public class QuizQuestionForAttemptDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = null!; // MultipleChoice, TrueFalse, etc.
    public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
}

public class QuizForAttemptDto
{
    public int QuizId { get; set; }
    public int DocumentId { get; set; }
    public string Title { get; set; } = null!;
    public string Difficulty { get; set; } = "";
    public IReadOnlyList<QuizQuestionForAttemptDto> Questions { get; set; } = Array.Empty<QuizQuestionForAttemptDto>();
}
