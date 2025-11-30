using System.Text.Json;
using MediatR;
using System.Text.RegularExpressions;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Flashcards.Commands;

internal class GenerateFlashcardsFromDocumentHandler 
    : IRequestHandler<GenerateFlashcardsFromDocumentCommand, IReadOnlyList<GeneratedFlashcardDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Summary> _summaryRepository;
    private readonly IRepository<Flashcard> _flashcardRepository;
    private readonly IGeminiService _geminiService;

    public GenerateFlashcardsFromDocumentHandler(
        IDocumentRepository documentRepository,
        IRepository<Summary> summaryRepository,
        IRepository<Flashcard> flashcardRepository,
        IGeminiService geminiService)
    {
        _documentRepository = documentRepository;
        _summaryRepository = summaryRepository;
        _flashcardRepository = flashcardRepository;
        _geminiService = geminiService;
    }

    // ✅ Limpia cualquier texto extra de Gemini y devuelve SOLO el array JSON
    private static string ExtractJsonArray(string raw)
    {
        var match = Regex.Match(raw, @"\[[\s\S]*\]", RegexOptions.Multiline);

        if (!match.Success)
            throw new InvalidOperationException("Gemini no devolvió un JSON válido para flashcards.");

        return match.Value;
    }

    public async Task<IReadOnlyList<GeneratedFlashcardDto>> Handle(
        GenerateFlashcardsFromDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null)
            throw new InvalidOperationException("Documento no encontrado.");

        var owns = await _documentRepository.UserOwnsDocumentAsync(
            request.DocumentId,
            request.UserId);

        if (!owns)
            throw new UnauthorizedAccessException("El documento no pertenece al usuario.");

        // 1) Fuente de datos
        var keyConceptSummaries = await _summaryRepository.FindAsync(
            s => s.DocumentId == document.Id && s.Type == "KeyConcepts");

        var keyConceptSummary = keyConceptSummaries
            .OrderByDescending(s => s.GeneratedAt ?? DateTime.MinValue)
            .FirstOrDefault();

        string sourceText;
        if (keyConceptSummary != null)
        {
            sourceText = keyConceptSummary.Content;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(document.ExtractedText))
                throw new InvalidOperationException(
                    "El documento no tiene conceptos clave ni texto extraído para generar flashcards.");

            sourceText = document.ExtractedText!;
        }

        // 2) Llamar a Gemini
        var jsonText = await _geminiService
            .GenerateFlashcardsJsonAsync(sourceText, cancellationToken);

        List<GeneratedFlashcardDto>? parsed;

        try
        {
            // ✅ Limpiar respuesta antes de deserializar
            var cleanJson = ExtractJsonArray(jsonText);

            parsed = JsonSerializer.Deserialize<List<GeneratedFlashcardDto>>(
                cleanJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "No se pudo interpretar la respuesta de Gemini para las flashcards.", ex);
        }

        if (parsed == null || parsed.Count == 0)
            throw new InvalidOperationException("Gemini no generó ninguna flashcard.");

        // 3) Persistir en BD
        var result = new List<GeneratedFlashcardDto>();
        var today = DateTime.UtcNow.Date;

        foreach (var item in parsed)
        {
            var difficulty = string.IsNullOrWhiteSpace(item.Difficulty)
                ? "Medium"
                : item.Difficulty;

            var flashcard = new Flashcard
            {
                Question = item.Question,
                Answer = item.Answer,
                Difficulty = difficulty,
                ReviewCount = 0,
                EaseFactor = 2.5,
                IntervalDays = 1,
                NextReviewDate = today,
                IsManuallyEdited = false,
                DocumentId = document.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _flashcardRepository.AddAsync(flashcard);

            result.Add(new GeneratedFlashcardDto
            {
                Question = flashcard.Question,
                Answer = flashcard.Answer,
                Difficulty = flashcard.Difficulty
            });
        }

        return result;
    }
}
