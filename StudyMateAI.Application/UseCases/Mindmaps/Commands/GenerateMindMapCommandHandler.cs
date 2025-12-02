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
        if (document == null) throw new InvalidOperationException("Documento no encontrado.");
        
        // (Agrega aquí tu validación de UserOwnsDocument si la tienes)

        if (string.IsNullOrWhiteSpace(document.ExtractedText))
            throw new InvalidOperationException("El documento no tiene texto.");

        // 2. Llamada EXCLUSIVA para MindMap (Jerárquico)
        var jsonResult = await _geminiService.GenerateMindMapJsonAsync(document.ExtractedText!, cancellationToken);

        int estimatedNodeCount = jsonResult.Split("label").Length - 1;

        // 3. Crear Entidad Mindmap
        var mindMap = new Domain.Entities.Mindmap
        {
            DocumentId = document.Id,
            Title = $"Mapa Mental: {document.FileName}",
            NodesJson = jsonResult,
            NodeCount = estimatedNodeCount,
            ColorScheme = request.ColorScheme ?? "default",
            AiModelUsed = "gemini-2.0-flash",
            JsonSchemaVersion = "1.0",
            GeneratedAt = DateTime.UtcNow
        };

        var created = await _mindMapRepository.AddAsync(mindMap);

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