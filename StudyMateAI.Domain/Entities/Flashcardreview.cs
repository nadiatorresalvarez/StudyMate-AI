using System;
using System.Collections.Generic;

namespace StudyMateAI.Domain.Entities;

public partial class Flashcardreview
{
    public int Id { get; set; }

    public int FlashcardId { get; set; }

    public int UserId { get; set; }

    /// <summary>
    /// Easy, Medium, Hard, Again (compatibilidad)
    /// </summary>
    public string Rating { get; set; } = null!;

    /// <summary>
    /// Calidad de la respuesta 0..5 para SRS
    /// </summary>
    public int Quality { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual Flashcard Flashcard { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
