using MediatR;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Application.DTOs.Mindmap;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Mindmaps.Commands;

internal class GenerateMindMapCommandHandler : IRequestHandler<GenerateMindMapCommand, GenerateMindMapResponseDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Domain.Entities.Mindmap> _mindMapRepository; // Especificamos el tipo completo por si acaso
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

        // 2. Llamar a Gemini (Usa el método nuevo que pusimos en GeminiService)
        var jsonResult = await _geminiService.GenerateMindMapJsonAsync(document.ExtractedText!, cancellationToken);

        // Intento simple de contar nodos contando cuántas veces aparece "label" en el JSON
        // (Esto es un aproximado rápido, no perfecto, pero útil)
        int estimatedNodeCount = jsonResult.Split("label").Length - 1;

        // 3. Crear la Entidad con TUS PROPIEDADES EXACTAS
        var mindMap = new Domain.Entities.Mindmap
        {
            DocumentId = document.Id,
            Title = $"Mapa Mental: {document.FileName}",
            NodesJson = jsonResult,       // <--- Aquí guardamos el JSON de Gemini
            NodeCount = estimatedNodeCount,
            ColorScheme = request.ColorScheme,
            AiModelUsed = "gemini-2.0-flash", // O el que use tu servicio
            JsonSchemaVersion = "1.0",
            GeneratedAt = DateTime.UtcNow,
            ImageUrl = null // Por ahora null, ya que generamos JSON, no imagen PNG
        };

        // 4. Guardar en Base de Datos
        var created = await _mindMapRepository.AddAsync(mindMap);

        // 5. Retornar DTO
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