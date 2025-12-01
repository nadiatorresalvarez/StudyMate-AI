using MediatR;
using StudyMateAI.Application.DTOs.Quizzes;

namespace StudyMateAI.Application.UseCases.Quizzes.Commands;

public class GenerateQuizFromDocumentCommand : IRequest<GenerateQuizResponseDto>
{
    public int UserId { get; set; }
    public int DocumentId { get; set; }
    public int QuestionCount { get; set; } = 10;
    public string Difficulty { get; set; } = "Medium";
}
