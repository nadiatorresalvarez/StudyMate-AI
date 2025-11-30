using MediatR;
using StudyMateAI.Application.DTOs.Flashcards;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Flashcards.Commands;

public record CreateFlashcardCommand(int UserId, int DocumentId, CreateFlashcardRequestDto Request) : IRequest<FlashcardResponseDto>;

public record UpdateFlashcardCommand(int UserId, int FlashcardId, UpdateFlashcardRequestDto Request) : IRequest<FlashcardResponseDto?>;

public record DeleteFlashcardCommand(int UserId, int FlashcardId) : IRequest<bool>;

public record GetFlashcardsByDocumentCommand(int UserId, int DocumentId) : IRequest<IReadOnlyList<FlashcardResponseDto>>;

public class CreateFlashcardCommandHandler : IRequestHandler<CreateFlashcardCommand, FlashcardResponseDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Flashcard> _flashcardRepository;

    public CreateFlashcardCommandHandler(IDocumentRepository documentRepository, IRepository<Flashcard> flashcardRepository)
    {
        _documentRepository = documentRepository;
        _flashcardRepository = flashcardRepository;
    }

    public async Task<FlashcardResponseDto> Handle(CreateFlashcardCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null)
            throw new InvalidOperationException("Documento no encontrado.");

        var owns = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
        if (!owns)
            throw new UnauthorizedAccessException("El documento no pertenece al usuario.");

        var dto = request.Request;

        var flashcard = new Flashcard
        {
            Question = dto.Question,
            Answer = dto.Answer,
            Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? "Medium" : dto.Difficulty,
            ReviewCount = 0,
            EaseFactor = 2.5,
            IntervalDays = 1,
            NextReviewDate = DateTime.UtcNow.Date,
            IsManuallyEdited = true,
            DocumentId = document.Id,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _flashcardRepository.AddAsync(flashcard);

        return new FlashcardResponseDto
        {
            Id = created.Id,
            Question = created.Question,
            Answer = created.Answer,
            Difficulty = created.Difficulty,
            DocumentId = created.DocumentId
        };
    }
}

public class UpdateFlashcardCommandHandler : IRequestHandler<UpdateFlashcardCommand, FlashcardResponseDto?>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Flashcard> _flashcardRepository;

    public UpdateFlashcardCommandHandler(IDocumentRepository documentRepository, IRepository<Flashcard> flashcardRepository)
    {
        _documentRepository = documentRepository;
        _flashcardRepository = flashcardRepository;
    }

    public async Task<FlashcardResponseDto?> Handle(UpdateFlashcardCommand request, CancellationToken cancellationToken)
    {
        var flashcard = await _flashcardRepository.GetByIdAsync(request.FlashcardId);
        if (flashcard == null)
            return null;

        var owns = await _documentRepository.UserOwnsDocumentAsync(flashcard.DocumentId, request.UserId);
        if (!owns)
            throw new UnauthorizedAccessException("La flashcard no pertenece a un documento del usuario.");

        var dto = request.Request;

        if (!string.IsNullOrWhiteSpace(dto.Question))
            flashcard.Question = dto.Question;

        if (!string.IsNullOrWhiteSpace(dto.Answer))
            flashcard.Answer = dto.Answer;

        if (!string.IsNullOrWhiteSpace(dto.Difficulty))
            flashcard.Difficulty = dto.Difficulty!;

        flashcard.IsManuallyEdited = true;

        var updated = await _flashcardRepository.UpdateAsync(flashcard);

        return new FlashcardResponseDto
        {
            Id = updated.Id,
            Question = updated.Question,
            Answer = updated.Answer,
            Difficulty = updated.Difficulty,
            DocumentId = updated.DocumentId
        };
    }
}

public class DeleteFlashcardCommandHandler : IRequestHandler<DeleteFlashcardCommand, bool>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Flashcard> _flashcardRepository;

    public DeleteFlashcardCommandHandler(IDocumentRepository documentRepository, IRepository<Flashcard> flashcardRepository)
    {
        _documentRepository = documentRepository;
        _flashcardRepository = flashcardRepository;
    }

    public async Task<bool> Handle(DeleteFlashcardCommand request, CancellationToken cancellationToken)
    {
        var flashcard = await _flashcardRepository.GetByIdAsync(request.FlashcardId);
        if (flashcard == null)
            return false;

        var owns = await _documentRepository.UserOwnsDocumentAsync(flashcard.DocumentId, request.UserId);
        if (!owns)
            throw new UnauthorizedAccessException("La flashcard no pertenece a un documento del usuario.");

        return await _flashcardRepository.DeleteAsync(request.FlashcardId);
    }
}

public class GetFlashcardsByDocumentCommandHandler : IRequestHandler<GetFlashcardsByDocumentCommand, IReadOnlyList<FlashcardResponseDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Flashcard> _flashcardRepository;

    public GetFlashcardsByDocumentCommandHandler(IDocumentRepository documentRepository, IRepository<Flashcard> flashcardRepository)
    {
        _documentRepository = documentRepository;
        _flashcardRepository = flashcardRepository;
    }

    public async Task<IReadOnlyList<FlashcardResponseDto>> Handle(GetFlashcardsByDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null)
            throw new InvalidOperationException("Documento no encontrado.");

        var owns = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
        if (!owns)
            throw new UnauthorizedAccessException("El documento no pertenece al usuario.");

        var flashcards = await _flashcardRepository.FindAsync(f => f.DocumentId == request.DocumentId);

        return flashcards
            .Select(f => new FlashcardResponseDto
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                Difficulty = f.Difficulty,
                DocumentId = f.DocumentId
            })
            .ToList();
    }
}
