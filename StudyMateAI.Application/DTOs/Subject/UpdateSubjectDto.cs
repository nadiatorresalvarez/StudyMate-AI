using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.Application.DTOs.Subject
{
    public class UpdateSubjectDto
    {
        [Required(ErrorMessage = "El nombre de la materia es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "El color debe ser un código hexadecimal válido (ej: #3B82F6)")]
        public string? Color { get; set; }

        [StringLength(50, ErrorMessage = "El icono no puede exceder 50 caracteres")]
        public string? Icon { get; set; }

        public int? OrderIndex { get; set; }

        public bool? IsArchived { get; set; }
    }
}

