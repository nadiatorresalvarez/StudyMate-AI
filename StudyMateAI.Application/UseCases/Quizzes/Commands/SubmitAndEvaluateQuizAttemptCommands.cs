using System.Text.Json;
using MediatR;
using StudyMateAI.Application.DTOs.Quizzes;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace StudyMateAI.Application.UseCases.Quizzes.Commands;

public record SubmitQuizAttemptCommand(int UserId, int QuizId, SubmitQuizAttemptRequestDto Request) : IRequest<int>;

public record EvaluateQuizAttemptCommand(int UserId, int AttemptId) : IRequest<QuizAttemptResultDto?>;

internal class SubmitQuizAttemptCommandHandler : IRequestHandler<SubmitQuizAttemptCommand, int>
{
    private readonly IRepository<Quiz> _quizRepository;
    private readonly IRepository<Quizattempt> _attemptRepository;

    public SubmitQuizAttemptCommandHandler(IRepository<Quiz> quizRepository, IRepository<Quizattempt> attemptRepository)
    {
        _quizRepository = quizRepository;
        _attemptRepository = attemptRepository;
    }

    public async Task<int> Handle(SubmitQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository
            .Query()
            .Include(q => q.Document)
            .ThenInclude(d => d.Subject)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == request.QuizId);

        if (quiz == null)
            throw new InvalidOperationException("Quiz no encontrado.");

        if (quiz.Document == null || quiz.Document.Subject == null || quiz.Document.Subject.UserId != request.UserId)
            throw new UnauthorizedAccessException("El quiz no pertenece a un documento del usuario.");

        var answersJson = JsonSerializer.Serialize(request.Request.Answers);

        // Calcular número de intento
        var currentAttempts = quiz.Quizattempts?.Count(a => a.UserId == request.UserId) ?? 0;
        var attemptNumber = currentAttempts + 1;

        var now = DateTime.UtcNow;

        var attempt = new Quizattempt
        {
            QuizId = quiz.Id,
            UserId = request.UserId,
            Score = 0,
            CorrectAnswers = 0,
            TotalQuestions = quiz.TotalQuestions,
            AnswersJson = answersJson,
            AttemptNumber = attemptNumber,
            TimeSpentSeconds = null,
            StartedAt = now,
            CompletedAt = null,
            IsLatest = true
        };

        attempt = await _attemptRepository.AddAsync(attempt);

        return attempt.Id;
    }
}

internal class EvaluateQuizAttemptCommandHandler : IRequestHandler<EvaluateQuizAttemptCommand, QuizAttemptResultDto?>
{
    private readonly IRepository<Quizattempt> _attemptRepository;
    private readonly IRepository<Quiz> _quizRepository;

    public EvaluateQuizAttemptCommandHandler(IRepository<Quizattempt> attemptRepository, IRepository<Quiz> quizRepository)
    {
        _attemptRepository = attemptRepository;
        _quizRepository = quizRepository;
    }

    public async Task<QuizAttemptResultDto?> Handle(EvaluateQuizAttemptCommand request, CancellationToken cancellationToken)
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
        var correctCount = 0;

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
                // Por ahora, respuestas abiertas se tratan como incorrectas
                isCorrect = false;
            }

            if (isCorrect)
                correctCount++;

            questionResults.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                SelectedOption = submitted?.SelectedOption,
                SelectedAnswerText = submitted?.AnswerText,
                CorrectAnswer = question.CorrectAnswer,
                IsCorrect = isCorrect,
                Options = !string.IsNullOrEmpty(question.OptionsJson)
                ? JsonSerializer.Deserialize<List<string>>(question.OptionsJson) ?? new List<string>()
                : new List<string>()
            });
        }

        var totalQuestions = questions.Count;
        var scorePercent = totalQuestions > 0 ? (double)correctCount / totalQuestions * 100.0 : 0.0;

        var now = DateTime.UtcNow;
        attempt.CorrectAnswers = correctCount;
        attempt.TotalQuestions = totalQuestions;
        attempt.Score = (int)Math.Round(scorePercent);
        attempt.CompletedAt = now;
        attempt.TimeSpentSeconds = attempt.StartedAt == default ? null : (int?)(now - attempt.StartedAt).TotalSeconds;

        await _attemptRepository.UpdateAsync(attempt);

        return new QuizAttemptResultDto
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            DocumentId = quiz.DocumentId,
            QuizTitle = quiz.Title,
            CorrectAnswers = correctCount,
            TotalQuestions = totalQuestions,
            ScorePercent = scorePercent,
            TimeSpentSeconds = attempt.TimeSpentSeconds,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Questions = questionResults
        };
    }
}
