namespace StudyMateAI.Application.DTOs.Quizzes;

public class GenerateQuizRequestDto
{
    public int QuestionCount { get; set; } = 10;
    public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
}

public class GeneratedQuizOptionDto
{
    public string Text { get; set; } = null!;
}

public class GeneratedQuizQuestionDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = null!; // MultipleChoice, TrueFalse
    public IReadOnlyList<GeneratedQuizOptionDto> Options { get; set; } = Array.Empty<GeneratedQuizOptionDto>();
}

public class GenerateQuizResponseDto
{
    public int QuizId { get; set; }
    public int DocumentId { get; set; }
    public string Title { get; set; } = null!;
    public string Difficulty { get; set; } = null!;
    public int QuestionCount { get; set; }
    public IReadOnlyList<GeneratedQuizQuestionDto> Questions { get; set; } = Array.Empty<GeneratedQuizQuestionDto>();
}
