using MediatR;
using StudyMateAI.Application.DTOs.Flashcards;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Flashcards.Queries;

public record GetNextStudyFlashcardQuery(int UserId) : IRequest<NextStudyFlashcardDto?>;

public class GetNextStudyFlashcardQueryHandler : IRequestHandler<GetNextStudyFlashcardQuery, NextStudyFlashcardDto?>
{
    private readonly IRepository<Flashcard> _flashcardRepository;

    public GetNextStudyFlashcardQueryHandler(IRepository<Flashcard> flashcardRepository)
    {
        _flashcardRepository = flashcardRepository;
    }

    public async Task<NextStudyFlashcardDto?> Handle(GetNextStudyFlashcardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        // Buscar flashcards del usuario con NextReviewDate vencida o para hoy
        var candidates = await _flashcardRepository.FindAsync(f =>
            f.Document.Subject.UserId == request.UserId &&
            f.NextReviewDate <= today);

        var next = candidates
            .OrderBy(f => f.NextReviewDate)
            .ThenBy(f => f.Id)
            .FirstOrDefault();

        if (next == null)
            return null;

        return new NextStudyFlashcardDto
        {
            FlashcardId = next.Id,
            Question = next.Question,
            Answer = next.Answer,
            Difficulty = next.Difficulty,
            NextReviewDate = next.NextReviewDate,
            ReviewCount = next.ReviewCount,
            IntervalDays = next.IntervalDays,
            EaseFactor = next.EaseFactor
        };
    }
}
