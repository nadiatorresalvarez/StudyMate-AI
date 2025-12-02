using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;
using StudyMateAI.Application.Common.Abstractions;
using System;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Linq;

namespace StudyMateAI.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentResponseDto>
{
    private static readonly HashSet<string> AllowedExtensions = new(new[] { ".pdf", ".docx", ".pptx", ".txt" });
    private const int MaxFileSizeMb = 20;

    private readonly IDocumentRepository _documentRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IMapper _mapper;

    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        ISubjectRepository subjectRepository,
        IFileStorage fileStorage,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _subjectRepository = subjectRepository;
        _fileStorage = fileStorage;
        _mapper = mapper;
    }

    public async Task<DocumentResponseDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // Validar materia pertenece al usuario
        var subject = await _subjectRepository.GetByIdAndUserIdAsync(request.SubjectId, request.UserId);
        if (subject == null)
            throw new UnauthorizedAccessException("La materia no existe o no pertenece al usuario");

        // Límite por usuario (50 documentos)
        var count = await _documentRepository.CountByUserAsync(request.UserId, cancellationToken);
        if (count >= 50)
            throw new InvalidOperationException("Se alcanzó el límite de 50 documentos por usuario");

        // Validaciones de archivo
        var ext = System.IO.Path.GetExtension(request.OriginalFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Extensión de archivo no permitida. Use .pdf, .docx, .pptx o .txt");

        var maxBytes = MaxFileSizeMb * 1024L * 1024L;
        if (request.Size <= 0 || request.Size > maxBytes)
            throw new InvalidOperationException($"El archivo supera el tamaño máximo de {MaxFileSizeMb} MB");

        // Nombre físico único y ruta relativa por usuario/materia
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var relativePath = $"{request.UserId}/{request.SubjectId}";

        using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        var fileUrl = await _fileStorage.SaveAsync(buffer, uniqueName, request.ContentType, relativePath, cancellationToken);
        buffer.Position = 0;

        // Mapear FileType desde extensión
        string fileType = ext.Trim('.').ToUpperInvariant();

        string extractedText = string.Empty;
        try
        {
            extractedText = ExtractText(buffer, ext);
        }
        catch (Exception)
        {
            extractedText = string.Empty;
        }

        var document = new Document
        {
            FileName = uniqueName,
            OriginalFileName = request.OriginalFileName,
            FileType = fileType,
            FileUrl = fileUrl,
            FileSizeKb = (int?)Math.Ceiling(request.Size / 1024.0),
            SubjectId = request.SubjectId,
            ProcessingStatus = string.IsNullOrEmpty(extractedText) ? "Failed" : "Completed",
            ProcessingError = string.IsNullOrEmpty(extractedText) ? "Extraction failed" : null,
            UploadedAt = DateTime.UtcNow,
            ExtractedText = extractedText,
            ProcessedAt = DateTime.UtcNow
        };

        var created = await _documentRepository.AddAsync(document);

        var response = _mapper.Map<DocumentResponseDto>(created);
        response.SubjectName = subject.Name;
        response.FlashcardCount = 0;
        response.QuizCount = 0;
        response.SummaryCount = 0;
        response.MindmapCount = 0;
        response.ConceptmapCount = 0;
        return response;
    }

    private static string ExtractText(Stream stream, string ext)
    {
        stream.Position = 0;
        switch (ext)
        {
            case ".pdf":
                using (var reader = new PdfReader(stream))
                using (var pdf = new PdfDocument(reader))
                {
                    var pages = Enumerable.Range(1, pdf.GetNumberOfPages())
                        .Select(i => PdfTextExtractor.GetTextFromPage(pdf.GetPage(i)));
                    return string.Join("\n", pages);
                }
            case ".docx":
                using (var word = WordprocessingDocument.Open(stream, false))
                {
                    var body = word.MainDocumentPart?.Document?.Body;
                    return body?.InnerText ?? string.Empty;
                }
            case ".pptx":
                using (var pres = PresentationDocument.Open(stream, false))
                {
                    var texts = pres.PresentationPart?
                        .SlideParts?
                        .SelectMany(sp => sp.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                        .Select(t => t.Text) ?? Enumerable.Empty<string>();
                    return string.Join("\n", texts);
                }
            case ".txt":
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            default:
                return string.Empty;
        }
    }
}
