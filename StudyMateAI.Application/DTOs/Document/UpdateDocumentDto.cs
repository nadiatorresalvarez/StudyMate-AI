using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.Application.DTOs.Document
{
    public class UpdateDocumentDto
    {
        [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
        public string? FileName { get; set; }

        public string? ExtractedText { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El tamaño del archivo debe ser mayor a 0")]
        public int? FileSizeKb { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número de páginas debe ser mayor a 0")]
        public int? PageCount { get; set; }

        [StringLength(10, ErrorMessage = "El código de idioma no puede exceder 10 caracteres")]
        public string? Language { get; set; }

        [RegularExpression(@"^(Pending|Completed|Failed)$", ErrorMessage = "Estado no válido. Debe ser: Pending, Completed o Failed")]
        public string? ProcessingStatus { get; set; }

        public string? ProcessingError { get; set; }
    }
}

