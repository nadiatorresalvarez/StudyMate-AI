using System.Text.Json;
using MediatR;
using StudyMateAI.Application.DTOs.Quizzes;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StudyMateAI.Application.UseCases.Quizzes.Queries;

public record GetQuizAttemptResultQuery(int UserId, int AttemptId) : IRequest<QuizAttemptResultDto?>;

public record GetQuizHistoryQuery(int UserId, int? DocumentId, int? QuizId) : IRequest<QuizHistoryResponseDto>;

internal class GetQuizAttemptResultQueryHandler : IRequestHandler<GetQuizAttemptResultQuery, QuizAttemptResultDto?>
{
    private readonly IRepository<Quizattempt> _attemptRepository;
    private readonly IRepository<Quiz> _quizRepository;

    public GetQuizAttemptResultQueryHandler(IRepository<Quizattempt> attemptRepository, IRepository<Quiz> quizRepository)
    {
        _attemptRepository = attemptRepository;
        _quizRepository = quizRepository;
    }

    public async Task<QuizAttemptResultDto?> Handle(GetQuizAttemptResultQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _attemptRepository.GetByIdAsync(request.AttemptId);
        if (attempt == null)
            return null;

        if (attempt.UserId != request.UserId)
            throw new UnauthorizedAccessException("El intento no pertenece al usuario.");

        var quiz = await _quizRepository
            .Query()
            .Include(q => q.Document)
                .ThenInclude(d => d.Subject)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == attempt.QuizId, cancellationToken);

        if (quiz == null)
            throw new InvalidOperationException("Quiz no encontrado para el intento.");

        if (quiz.Document == null || quiz.Document.Subject == null || quiz.Document.Subject.UserId != request.UserId)
            throw new UnauthorizedAccessException("El quiz no pertenece a un documento del usuario.");

        var submittedAnswers = string.IsNullOrWhiteSpace(attempt.AnswersJson)
            ? new List<SubmittedAnswerDto>()
            : JsonSerializer.Deserialize<List<SubmittedAnswerDto>>(attempt.AnswersJson) ?? new List<SubmittedAnswerDto>();

        var questions = quiz.Questions.OrderBy(q => q.OrderIndex).ToList();

        var questionResults = new List<QuestionResultDto>();

        foreach (var question in questions)
        {
            var submitted = submittedAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

            var isCorrect = false;

            if (question.QuestionType == "MultipleChoice" || question.QuestionType == "TrueFalse")
            {
                if (!string.IsNullOrWhiteSpace(submitted?.SelectedOption) &&
                    string.Equals(submitted.SelectedOption?.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    isCorrect = true;
                }
            }
            else
            {
                isCorrect = false;
            }

            questionResults.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                SelectedOption = submitted?.SelectedOption,
                SelectedAnswerText = submitted?.AnswerText,
                CorrectAnswer = question.CorrectAnswer,
                IsCorrect = isCorrect
            });
        }

        var totalQuestions = questions.Count;
        var correctAnswers = attempt.CorrectAnswers;
        var scorePercent = totalQuestions > 0
            ? (double)correctAnswers / totalQuestions * 100.0
            : 0.0;

        return new QuizAttemptResultDto
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            DocumentId = quiz.DocumentId,
            QuizTitle = quiz.Title,
            CorrectAnswers = correctAnswers,
            TotalQuestions = totalQuestions,
            ScorePercent = scorePercent,
            TimeSpentSeconds = attempt.TimeSpentSeconds,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Questions = questionResults
        };
    }
}

internal class GetQuizHistoryQueryHandler : IRequestHandler<GetQuizHistoryQuery, QuizHistoryResponseDto>
{
    private readonly IRepository<Quizattempt> _attemptRepository;
    private readonly IRepository<Quiz> _quizRepository;

    public GetQuizHistoryQueryHandler(IRepository<Quizattempt> attemptRepository, IRepository<Quiz> quizRepository)
    {
        _attemptRepository = attemptRepository;
        _quizRepository = quizRepository;
    }

    public async Task<QuizHistoryResponseDto> Handle(GetQuizHistoryQuery request, CancellationToken cancellationToken)
    {
        // Cargar intentos del usuario (se filtra en memoria por DocumentId/QuizId si es necesario)
        var attempts = await _attemptRepository.FindAsync(a => a.UserId == request.UserId);

        var filtered = attempts
            .Where(a => !request.QuizId.HasValue || a.QuizId == request.QuizId.Value)
            .ToList();

        // Cargar quizzes necesarios en un diccionario para evitar múltiples consultas
        var quizIds = filtered.Select(a => a.QuizId).Distinct().ToList();
        var quizzes = new Dictionary<int, Quiz>();
        foreach (var quizId in quizIds)
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            if (quiz != null)
            {
                quizzes[quizId] = quiz;
            }
        }

        // Aplicar filtro por DocumentId si corresponde
        if (request.DocumentId.HasValue)
        {
            var docId = request.DocumentId.Value;
            filtered = filtered
                .Where(a => quizzes.TryGetValue(a.QuizId, out var q) && q.DocumentId == docId)
                .ToList();
        }

        // Construir summaries
        var summaries = new List<QuizAttemptSummaryDto>();
        foreach (var attempt in filtered)
        {
            if (!quizzes.TryGetValue(attempt.QuizId, out var quiz))
                continue;

            var totalQuestions = attempt.TotalQuestions;
            var correct = attempt.CorrectAnswers;
            var scorePercent = totalQuestions > 0
                ? (double)correct / totalQuestions * 100.0
                : 0.0;

            summaries.Add(new QuizAttemptSummaryDto
            {
                AttemptId = attempt.Id,
                QuizId = quiz.Id,
                DocumentId = quiz.DocumentId,
                QuizTitle = quiz.Title,
                CorrectAnswers = correct,
                TotalQuestions = totalQuestions,
                ScorePercent = scorePercent,
                TimeSpentSeconds = attempt.TimeSpentSeconds,
                CompletedAt = attempt.CompletedAt
            });
        }

        // Calcular WeakTopics: agrupamos por un "topic" derivado.
        // Como el dominio no tiene Question.Topic, usaremos el título del quiz como proxy de tema.
        var topicStats = new Dictionary<string, (int failed, int total)>();

        foreach (var attempt in filtered)
        {
            if (!quizzes.TryGetValue(attempt.QuizId, out var quiz))
                continue;

            var submittedAnswers = string.IsNullOrWhiteSpace(attempt.AnswersJson)
                ? new List<SubmittedAnswerDto>()
                : JsonSerializer.Deserialize<List<SubmittedAnswerDto>>(attempt.AnswersJson) ?? new List<SubmittedAnswerDto>();

            var questions = quiz.Questions.OrderBy(q => q.OrderIndex).ToList();
            var topic = quiz.Title; // proxy de Topic

            foreach (var question in questions)
            {
                var submitted = submittedAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

                var isCorrect = false;

                if (question.QuestionType == "MultipleChoice" || question.QuestionType == "TrueFalse")
                {
                    if (!string.IsNullOrWhiteSpace(submitted?.SelectedOption) &&
                        string.Equals(submitted.SelectedOption?.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        isCorrect = true;
                    }
                }
                else
                {
                    isCorrect = false;
                }

                if (!topicStats.TryGetValue(topic, out var stat))
                    stat = (0, 0);

                stat.total++;
                if (!isCorrect)
                    stat.failed++;

                topicStats[topic] = stat;
            }
        }

        var weakTopics = topicStats
            .Select(kvp => new WeakTopicDto
            {
                Topic = kvp.Key,
                FailedCount = kvp.Value.failed,
                TotalCount = kvp.Value.total,
                FailureRate = kvp.Value.total > 0
                    ? (double)kvp.Value.failed / kvp.Value.total
                    : 0.0
            })
            .OrderByDescending(t => t.FailureRate)
            .ToList();

        return new QuizHistoryResponseDto
        {
            Attempts = summaries,
            WeakTopics = weakTopics
        };
    }
}
