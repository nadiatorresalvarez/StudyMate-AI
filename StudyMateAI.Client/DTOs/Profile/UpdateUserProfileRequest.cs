using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.Client.DTOs.Profile;

public class UpdateUserProfileRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Debe tener entre 3 y 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nivel educativo es obligatorio")]
    public string EducationLevel { get; set; } = string.Empty;
}