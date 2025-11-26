using System;
using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.Application.DTOs.Document
{
    public class UpdateProcessingStatusDto
    {
        [Required(ErrorMessage = "El estado de procesamiento es obligatorio")]
        [RegularExpression(@"^(Pending|Completed|Failed)$", ErrorMessage = "Estado no válido. Debe ser: Pending, Completed o Failed")]
        public string ProcessingStatus { get; set; } = null!;

        public string? ProcessingError { get; set; }

        public DateTime? ProcessedAt { get; set; }
    }
}
