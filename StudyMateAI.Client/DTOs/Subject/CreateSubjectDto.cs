using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.Client.DTOs.Subject;

public class CreateSubjectDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Mínimo 3 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public string? Color { get; set; } = "#2196F3"; // Azul por defecto

    public string? Icon { get; set; }
}