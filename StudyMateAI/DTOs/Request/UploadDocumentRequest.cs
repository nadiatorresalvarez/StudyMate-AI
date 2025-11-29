using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudyMateAI.DTOs.Request;

public class UploadDocumentRequest
{
    [Required]
    public IFormFile File { get; set; } = default!;

    [Required]
    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }
}