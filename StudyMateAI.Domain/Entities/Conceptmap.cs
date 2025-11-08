using System;
using System.Collections.Generic;

namespace StudyMateAI.Domain.Entities;

public partial class Conceptmap
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    /// <summary>
    /// Nodos del mapa conceptual
    /// </summary>
    public string NodesJson { get; set; } = null!;

    /// <summary>
    /// Relaciones entre nodos
    /// </summary>
    public string EdgesJson { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public int? NodeCount { get; set; }

    public int? EdgeCount { get; set; }

    public string? JsonSchemaVersion { get; set; }

    public string? AiModelUsed { get; set; }

    public int DocumentId { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public virtual Document Document { get; set; } = null!;
}
