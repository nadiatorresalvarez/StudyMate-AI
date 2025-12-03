namespace StudyMateAI.Client.DTOs.Quiz;

public class SubmitQuizAttemptRequestDto
{
    public List<SubmittedAnswerDto> Answers { get; set; } = new();
}