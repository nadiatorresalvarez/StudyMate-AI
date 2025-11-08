using System;
using System.Collections.Generic;

namespace StudyMateAI.Infrastructure.Models;

public partial class Flashcardreview
{
    public int Id { get; set; }

    public int FlashcardId { get; set; }

    public int UserId { get; set; }

    /// <summary>
    /// Easy, Medium, Hard, Again
    /// </summary>
    public string Rating { get; set; } = null!;

    public DateTime? ReviewedAt { get; set; }

    public virtual Flashcard Flashcard { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
