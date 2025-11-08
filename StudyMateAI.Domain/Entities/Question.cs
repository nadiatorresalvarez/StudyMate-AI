using System;
using System.Collections.Generic;

namespace StudyMateAI.Domain.Entities;

public partial class Question
{
    public int Id { get; set; }

    public string QuestionText { get; set; } = null!;

    /// <summary>
    /// MultipleChoice, TrueFalse, ShortAnswer
    /// </summary>
    public string QuestionType { get; set; } = null!;

    public string CorrectAnswer { get; set; } = null!;

    /// <summary>
    /// JSON con las opciones para multiple choice
    /// </summary>
    public string? OptionsJson { get; set; }

    public string? Explanation { get; set; }

    public int? Points { get; set; }

    public int OrderIndex { get; set; }

    public int QuizId { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;
}
