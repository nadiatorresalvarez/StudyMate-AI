using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.Client.DTOs.Subject;

public class UpdateSubjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }
        
    public bool? IsArchived { get; set; }
}