using AutoMapper;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;
using StudyMateAI.Application.Common.Abstractions;
using System;

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

        // Guardar archivo
        var fileUrl = await _fileStorage.SaveAsync(request.Content, uniqueName, request.ContentType, relativePath, cancellationToken);

        // Mapear FileType desde extensión
        string fileType = ext.Trim('.').ToUpperInvariant();

        // Crear entidad y persistir
        var document = new Document
        {
            FileName = uniqueName,
            OriginalFileName = request.OriginalFileName,
            FileType = fileType,
            FileUrl = fileUrl,
            FileSizeKb = (int?)Math.Ceiling(request.Size / 1024.0),
            SubjectId = request.SubjectId,
            ProcessingStatus = "Pending",
            UploadedAt = DateTime.UtcNow
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
}