using System;
using System.Collections.Generic;

namespace StudyMateAI.Infrastructure.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// Código hexadecimal: #3B82F6
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Emoji o nombre de icono
    /// </summary>
    public string? Icon { get; set; }

    public int? OrderIndex { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsArchived { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual User User { get; set; } = null!;
}
