using System;
using System.Collections.Generic;

namespace StudyMateAI.Infrastructure.Models;

public partial class Summary
{
    public int Id { get; set; }

    /// <summary>
    /// Brief, Detailed, KeyConcepts
    /// </summary>
    public string Type { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int? WordCount { get; set; }

    public bool? IsFavorite { get; set; }

    public int? RegenerationCount { get; set; }

    public string? AiModelUsed { get; set; }

    public int DocumentId { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public virtual Document Document { get; set; } = null!;
}
