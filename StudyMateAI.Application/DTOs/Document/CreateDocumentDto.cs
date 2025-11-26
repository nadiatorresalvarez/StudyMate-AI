using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.Application.DTOs.Document
{
    public class CreateDocumentDto
    {
        [Required(ErrorMessage = "El nombre del archivo es obligatorio")]
        [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
        public string FileName { get; set; } = null!;

        [Required(ErrorMessage = "El nombre original del archivo es obligatorio")]
        [StringLength(255, ErrorMessage = "El nombre original no puede exceder 255 caracteres")]
        public string OriginalFileName { get; set; } = null!;

        [Required(ErrorMessage = "El tipo de archivo es obligatorio")]
        [RegularExpression(@"^(PDF|DOCX|PPTX|TXT)$", ErrorMessage = "Tipo de archivo no válido. Debe ser: PDF, DOCX, PPTX o TXT")]
        public string FileType { get; set; } = null!;

        [Required(ErrorMessage = "La URL del archivo es obligatoria")]
        [Url(ErrorMessage = "La URL del archivo no es válida")]
        public string FileUrl { get; set; } = null!;

        [Required(ErrorMessage = "El ID de la materia es obligatorio")]
        public int SubjectId { get; set; }

        public string? ExtractedText { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El tamaño del archivo debe ser mayor a 0")]
        public int? FileSizeKb { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número de páginas debe ser mayor a 0")]
        public int? PageCount { get; set; }

        [StringLength(10, ErrorMessage = "El código de idioma no puede exceder 10 caracteres")]
        public string? Language { get; set; }
    }
}