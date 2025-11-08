using System;
using System.Collections.Generic;

namespace StudyMateAI.Infrastructure.Models;

public partial class Quizattempt
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public int UserId { get; set; }

    public int Score { get; set; }

    public int CorrectAnswers { get; set; }

    public int TotalQuestions { get; set; }

    /// <summary>
    /// JSON con todas las respuestas del usuario
    /// </summary>
    public string? AnswersJson { get; set; }

    public int AttemptNumber { get; set; }

    public int? TimeSpentSeconds { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool? IsLatest { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
