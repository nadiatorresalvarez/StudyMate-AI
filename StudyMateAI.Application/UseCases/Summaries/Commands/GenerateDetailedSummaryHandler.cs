using MediatR;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Application.DTOs.Summary;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Summaries.Commands;

internal class GenerateDetailedSummaryCommandHandler : IRequestHandler<GenerateDetailedSummaryCommand, GenerateBriefSummaryResponseDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Summary> _summaryRepository;
    private readonly IGeminiService _geminiService;

    public GenerateDetailedSummaryCommandHandler(
        IDocumentRepository documentRepository,
        IRepository<Summary> summaryRepository,
        IGeminiService geminiService)
    {
        _documentRepository = documentRepository;
        _summaryRepository = summaryRepository;
        _geminiService = geminiService;
    }

    public async Task<GenerateBriefSummaryResponseDto> Handle(GenerateDetailedSummaryCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null)
        {
            throw new InvalidOperationException("Documento no encontrado.");
        }

        var owns = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
        if (!owns)
        {
            throw new UnauthorizedAccessException("El documento no pertenece al usuario.");
        }

        if (string.IsNullOrWhiteSpace(document.ExtractedText))
        {
            throw new InvalidOperationException("El documento no tiene texto extraído para resumir.");
        }

        var summaryText = await _geminiService.GenerateDetailedSummaryAsync(document.ExtractedText!, cancellationToken);

        var wordCount = summaryText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;

        var summary = new Summary
        {
            Type = "Detailed",
            Content = summaryText,
            WordCount = wordCount,
            IsFavorite = false,
            RegenerationCount = 1,
            AiModelUsed = "gemini-2.0-flash",
            DocumentId = document.Id,
            GeneratedAt = DateTime.UtcNow
        };

        var created = await _summaryRepository.AddAsync(summary);

        return new GenerateBriefSummaryResponseDto
        {
            DocumentId = created.DocumentId,
            SummaryId = created.Id,
            SummaryText = created.Content,
            CreatedAt = created.GeneratedAt ?? DateTime.UtcNow
        };
    }
}
