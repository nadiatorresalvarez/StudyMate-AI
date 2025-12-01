using System.Text.Json;
using MediatR;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Application.DTOs.Quizzes;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Quizzes.Commands;

internal class GenerateQuizFromDocumentHandler : IRequestHandler<GenerateQuizFromDocumentCommand, GenerateQuizResponseDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Quiz> _quizRepository;
    private readonly IRepository<Question> _questionRepository;
    private readonly IGeminiService _geminiService;

    public GenerateQuizFromDocumentHandler(
        IDocumentRepository documentRepository,
        IRepository<Quiz> quizRepository,
        IRepository<Question> questionRepository,
        IGeminiService geminiService)
    {
        _documentRepository = documentRepository;
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _geminiService = geminiService;
    }

    public async Task<GenerateQuizResponseDto> Handle(GenerateQuizFromDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null)
            throw new InvalidOperationException("Documento no encontrado.");

        var owns = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
        if (!owns)
            throw new UnauthorizedAccessException("El documento no pertenece al usuario.");

        if (string.IsNullOrWhiteSpace(document.ExtractedText))
            throw new InvalidOperationException("El documento no tiene texto extraído para generar un quiz.");

        var raw = await _geminiService.GenerateQuizJsonAsync(
    document.ExtractedText!,
    request.QuestionCount,
    request.Difficulty,
    cancellationToken);

// 🔍 LIMPIEZA ROBUSTA DEL TEXTO GEMINI
var cleanedJson = raw
    .Replace("```json", "")
    .Replace("```", "")
    .Trim();

var firstBracket = cleanedJson.IndexOf("[");
var lastBracket = cleanedJson.LastIndexOf("]");

if (firstBracket == -1 || lastBracket == -1)
{
    throw new InvalidOperationException("Gemini no retornó un JSON válido.");
}

cleanedJson = cleanedJson.Substring(
    firstBracket, 
    lastBracket - firstBracket + 1
);

List<GeminiQuizQuestion>? parsed;

try
{
    parsed = JsonSerializer.Deserialize<List<GeminiQuizQuestion>>(cleanedJson, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
}
catch (JsonException ex)
{
    throw new InvalidOperationException(
        "No se pudo interpretar la respuesta limpia de Gemini para el quiz.\nRAW:\n" + raw,
        ex
    );
}


        if (parsed == null || parsed.Count == 0)
            throw new InvalidOperationException("Gemini no generó ninguna pregunta para el quiz.");

        // Crear Quiz
        var quiz = new Quiz
        {
            Title = $"Quiz generado para {document.OriginalFileName}",
            TotalQuestions = parsed.Count,
            DocumentId = document.Id,
            GeneratedAt = DateTime.UtcNow,
            IsActive = true
        };

        quiz = await _quizRepository.AddAsync(quiz);

        var questionsDtos = new List<GeneratedQuizQuestionDto>();
        var order = 1;

        foreach (var q in parsed)
        {
            var options = q.Options ?? new List<string>();

            var question = new Question
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                CorrectAnswer = q.CorrectAnswer,
                OptionsJson = options.Count > 0 ? JsonSerializer.Serialize(options) : null,
                Explanation = q.Explanation,
                Points = 1,
                OrderIndex = order++,
                QuizId = quiz.Id
            };

            question = await _questionRepository.AddAsync(question);

            questionsDtos.Add(new GeneratedQuizQuestionDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Options = options.Select(o => new GeneratedQuizOptionDto { Text = o }).ToList()
            });
        }

        return new GenerateQuizResponseDto
        {
            QuizId = quiz.Id,
            DocumentId = quiz.DocumentId,
            Title = quiz.Title,
            Difficulty = request.Difficulty,
            QuestionCount = quiz.TotalQuestions,
            Questions = questionsDtos
        };
    }

    private class GeminiQuizQuestion
    {
        public string QuestionText { get; set; } = null!;
        public string QuestionType { get; set; } = null!;
        public List<string>? Options { get; set; }
        public string CorrectAnswer { get; set; } = null!;
        public string? Explanation { get; set; }
    }
}
