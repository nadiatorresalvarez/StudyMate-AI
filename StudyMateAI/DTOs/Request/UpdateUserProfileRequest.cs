using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.DTOs.Request;

public class UpdateUserProfileRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string EducationLevel { get; set; } = string.Empty;
}