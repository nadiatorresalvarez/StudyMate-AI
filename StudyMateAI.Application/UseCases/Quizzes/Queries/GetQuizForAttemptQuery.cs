using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyMateAI.Application.DTOs.Quizzes;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Quizzes.Queries;

public record GetQuizForAttemptQuery(int UserId, int QuizId) : IRequest<QuizForAttemptDto?>;

internal class GetQuizForAttemptQueryHandler : IRequestHandler<GetQuizForAttemptQuery, QuizForAttemptDto?>
{
    private readonly IRepository<Quiz> _quizRepository;

    public GetQuizForAttemptQueryHandler(IRepository<Quiz> quizRepository)
    {
        _quizRepository = quizRepository;
    }

    public async Task<QuizForAttemptDto?> Handle(GetQuizForAttemptQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository
            .Query()
            .Include(q => q.Document)
                .ThenInclude(d => d.Subject)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken);
        if (quiz == null)
            return null;

        // Validar que el quiz pertenece a un documento del usuario
        if (quiz.Document == null || quiz.Document.Subject == null || quiz.Document.Subject.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("El quiz no pertenece a un documento del usuario.");
        }

        var questions = quiz.Questions
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuizQuestionForAttemptDto
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = string.IsNullOrWhiteSpace(q.OptionsJson)
                    ? Array.Empty<string>()
                    : JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new List<string>()
            })
            .ToList();

        return new QuizForAttemptDto
        {
            QuizId = quiz.Id,
            DocumentId = quiz.DocumentId,
            Title = quiz.Title,
            Difficulty = string.Empty, // no hay campo de dificultad en Quiz de dominio
            Questions = questions
        };
    }
}
