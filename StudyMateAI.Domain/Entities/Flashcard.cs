using System;
using System.Collections.Generic;

namespace StudyMateAI.Domain.Entities;

public partial class Flashcard
{
    public int Id { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    /// <summary>
    /// Easy, Medium, Hard
    /// </summary>
    public string Difficulty { get; set; } = null!;

    public DateTime? NextReviewDate { get; set; }

    public int? ReviewCount { get; set; }

    public float? EaseFactor { get; set; }

    public int? IntervalDays { get; set; }

    public bool? IsManuallyEdited { get; set; }

    public int DocumentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastReviewedAt { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual ICollection<Flashcardreview> Flashcardreviews { get; set; } = new List<Flashcardreview>();
}
