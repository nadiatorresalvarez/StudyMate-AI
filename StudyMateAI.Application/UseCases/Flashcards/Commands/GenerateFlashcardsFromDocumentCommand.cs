using MediatR;

namespace StudyMateAI.Application.UseCases.Flashcards.Commands;

public class GenerateFlashcardsFromDocumentCommand : IRequest<IReadOnlyList<GeneratedFlashcardDto>>
{
    public int UserId { get; set; }
    public int DocumentId { get; set; }
}

public class GeneratedFlashcardDto
{
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public string Difficulty { get; set; } = null!;
}
