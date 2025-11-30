using MediatR;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Flashcards.Commands;

public record ReviewFlashcardCommand(int UserId, int FlashcardId, int Quality) : IRequest<bool>;

public class ReviewFlashcardCommandHandler : IRequestHandler<ReviewFlashcardCommand, bool>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Flashcard> _flashcardRepository;
    private readonly IRepository<Flashcardreview> _flashcardReviewRepository;

    public ReviewFlashcardCommandHandler(
        IDocumentRepository documentRepository,
        IRepository<Flashcard> flashcardRepository,
        IRepository<Flashcardreview> flashcardReviewRepository)
    {
        _documentRepository = documentRepository;
        _flashcardRepository = flashcardRepository;
        _flashcardReviewRepository = flashcardReviewRepository;
    }

    public async Task<bool> Handle(ReviewFlashcardCommand request, CancellationToken cancellationToken)
    {
        if (request.Quality < 0 || request.Quality > 5)
        {
            throw new InvalidOperationException("La calidad debe estar entre 0 y 5.");
        }

        var flashcard = await _flashcardRepository.GetByIdAsync(request.FlashcardId);
        if (flashcard == null)
        {
            return false;
        }

        var owns = await _documentRepository.UserOwnsDocumentAsync(flashcard.DocumentId, request.UserId);
        if (!owns)
        {
            throw new UnauthorizedAccessException("La flashcard no pertenece a un documento del usuario.");
        }

        // Registrar review
        var review = new Flashcardreview
        {
            FlashcardId = flashcard.Id,
            UserId = request.UserId,
            ReviewedAt = DateTime.UtcNow,
            Quality = request.Quality,
            Rating = string.Empty // no usado en la nueva lógica
        };

        await _flashcardReviewRepository.AddAsync(review);

        // Lógica SM-2 formal
        var quality = request.Quality;

        // Asegurar EaseFactor inicial razonable
        if (flashcard.EaseFactor <= 0)
        {
            flashcard.EaseFactor = 2.5;
        }

        if (quality < 3)
        {
            flashcard.ReviewCount = 0;
            flashcard.IntervalDays = 1;
        }
        else
        {
            flashcard.ReviewCount = (flashcard.ReviewCount ?? 0) + 1;

            if (flashcard.ReviewCount == 1)
                flashcard.IntervalDays = 1;
            else if (flashcard.ReviewCount == 2)
                flashcard.IntervalDays = 6;
            else
                flashcard.IntervalDays = (int)Math.Round((flashcard.IntervalDays ?? 1) * flashcard.EaseFactor);

            // Actualizar EaseFactor según SM-2
            flashcard.EaseFactor = flashcard.EaseFactor
                + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));

            if (flashcard.EaseFactor < 1.3)
                flashcard.EaseFactor = 1.3;
        }

        var interval = flashcard.IntervalDays ?? 1;
        flashcard.NextReviewDate = DateTime.UtcNow.Date.AddDays(interval);
        flashcard.LastReviewedAt = DateTime.UtcNow;

        await _flashcardRepository.UpdateAsync(flashcard);

        return true;
    }
}
