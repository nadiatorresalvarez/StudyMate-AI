using FluentValidation;
using StudyMateAI.DTOs.Request;
using System.IO;

namespace StudyMateAI.Validators;

public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
    private static readonly string[] AllowedExtensions = new[] { ".pdf", ".docx", ".pptx", ".txt" };
    private const int MaxFileSizeBytes = 20_000_000;

    public UploadDocumentRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(f => f.Length > 0)
            .WithMessage("El archivo no puede estar vacío")
            .Must(f => f.Length <= MaxFileSizeBytes)
            .WithMessage("El archivo supera el tamaño máximo de 20 MB")
            .Must(f => AllowedExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
            .WithMessage("Extensión no permitida. Use .pdf, .docx, .pptx o .txt");

        RuleFor(x => x.SubjectId)
            .GreaterThan(0);
    }
}
