using System;
using System.Collections.Generic;

namespace StudyMateAI.Infrastructure.Models;

public partial class Studysession
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? DocumentId { get; set; }

    /// <summary>
    /// Flashcards, Quiz, Reading, etc.
    /// </summary>
    public string ActivityType { get; set; } = null!;

    public int? DurationMinutes { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public virtual Document? Document { get; set; }

    public virtual User User { get; set; } = null!;
}
