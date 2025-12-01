using MediatR;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Application.DTOs.Mindmap; // Asegúrate que coincida con tu carpeta DTO
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Mindmaps.Commands;

internal class GenerateMindMapCommandHandler : IRequestHandler<GenerateMindMapCommand, GenerateMindMapResponseDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Domain.Entities.Mindmap> _mindMapRepository;
    private readonly IGeminiService _geminiService;

    public GenerateMindMapCommandHandler(
        IDocumentRepository documentRepository,
        IRepository<Domain.Entities.Mindmap> mindMapRepository,
        IGeminiService geminiService)
    {
        _documentRepository = documentRepository;
        _mindMapRepository = mindMapRepository;
        _geminiService = geminiService;
    }

    public async Task<GenerateMindMapResponseDto> Handle(GenerateMindMapCommand request, CancellationToken cancellationToken)
    {
        // 1. Validaciones
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null)
            throw new InvalidOperationException("Documento no encontrado.");

        var owns = await _documentRepository.UserOwnsDocumentAsync(request.DocumentId, request.UserId);
        if (!owns)
            throw new UnauthorizedAccessException("El documento no pertenece al usuario.");

        if (string.IsNullOrWhiteSpace(document.ExtractedText))
            throw new InvalidOperationException("El documento no tiene texto para procesar.");

        string mapType = "MindMap"; 
        
        var jsonResult = await _geminiService.GenerateMindMapJsonAsync(
            document.ExtractedText!, 
            mapType, 
            cancellationToken
        );

        int estimatedNodeCount = jsonResult.Split("label").Length - 1;

        // 3. Crear la Entidad
        var mindMap = new Domain.Entities.Mindmap
        {
            DocumentId = document.Id,
            Title = $"Mapa ({mapType}): {document.FileName}",
            NodesJson = jsonResult,
            NodeCount = estimatedNodeCount,
            ColorScheme = request.ColorScheme ?? "default", // Manejamos si viene nulo
            AiModelUsed = "gemini-2.0-flash",
            JsonSchemaVersion = "1.0",
            GeneratedAt = DateTime.UtcNow,
            ImageUrl = null
        };

        // 4. Guardar
        var created = await _mindMapRepository.AddAsync(mindMap);

        // 5. Retornar
        return new GenerateMindMapResponseDto
        {
            MindMapId = created.Id,
            DocumentId = created.DocumentId,
            Title = created.Title,
            NodesJson = created.NodesJson,
            NodeCount = created.NodeCount ?? 0,
            CreatedAt = created.GeneratedAt ?? DateTime.UtcNow
        };
    }
}