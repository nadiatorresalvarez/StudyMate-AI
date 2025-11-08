using System;
using System.Collections.Generic;
namespace StudyMateAI.Domain.Entities;

public partial class Mindmap
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    /// <summary>
    /// Estructura JSON del mapa mental
    /// </summary>
    public string NodesJson { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? ColorScheme { get; set; }

    public int? NodeCount { get; set; }

    public string? JsonSchemaVersion { get; set; }

    public string? AiModelUsed { get; set; }

    public int DocumentId { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public virtual Document Document { get; set; } = null!;
}
