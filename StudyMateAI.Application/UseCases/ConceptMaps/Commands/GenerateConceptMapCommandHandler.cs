using System.Text.Json;
using MediatR;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Application.DTOs.ConceptMap;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.ConceptMaps.Commands;

internal class GenerateConceptMapCommandHandler : IRequestHandler<GenerateConceptMapCommand, GenerateConceptMapResponseDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRepository<Domain.Entities.Conceptmap> _conceptMapRepository;
    private readonly IGeminiService _geminiService;

    public GenerateConceptMapCommandHandler(
        IDocumentRepository documentRepository,
        IRepository<Domain.Entities.Conceptmap> conceptMapRepository,
        IGeminiService geminiService)
    {
        _documentRepository = documentRepository;
        _conceptMapRepository = conceptMapRepository;
        _geminiService = geminiService;
    }

    public async Task<GenerateConceptMapResponseDto> Handle(GenerateConceptMapCommand request, CancellationToken ct)
    {
        // A. Validaciones
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null) throw new InvalidOperationException("Documento no encontrado.");

        // B. Llamar a Gemini
        string fullJson = await _geminiService.GenerateConceptMapJsonAsync(document.ExtractedText!, ct);

        // C. Separar el JSON
        string nodesStr = "[]";
        string edgesStr = "[]";
        int nodeCount = 0;
        int edgeCount = 0;

        try
        {
            var data = JsonSerializer.Deserialize<GeminiGraphResponse>(fullJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (data != null)
            {
                // Serializamos las listas de vuelta a string para guardar en BD
                nodesStr = JsonSerializer.Serialize(data.nodes);
                edgesStr = JsonSerializer.Serialize(data.edges);
                
                nodeCount = data.nodes?.Count ?? 0;
                edgeCount = data.edges?.Count ?? 0;
            }
        }
        catch 
        {
            nodesStr = fullJson; 
        }

        // D. Crear la Entidad
        var conceptMap = new Domain.Entities.Conceptmap
        {
            DocumentId = document.Id,
            Title = $"Mapa Conceptual: {document.FileName}",
            NodesJson = nodesStr,  
            EdgesJson = edgesStr,  
            NodeCount = nodeCount,
            EdgeCount = edgeCount,
            AiModelUsed = "gemini-2.0-flash",
            JsonSchemaVersion = "1.0",
            GeneratedAt = DateTime.UtcNow,
            ImageUrl = null
        };

        var created = await _conceptMapRepository.AddAsync(conceptMap);
        
        // ✅ SOLUCIÓN: Usar valores por defecto si la deserialización falla
        var cleanNodes = JsonSerializer.Deserialize<object>(created.NodesJson) ?? new object();
        var cleanEdges = JsonSerializer.Deserialize<object>(created.EdgesJson) ?? new object();

        return new GenerateConceptMapResponseDto
        {
            ConceptMapId = created.Id,
            Title = created.Title ?? string.Empty, // ✅ Protección adicional por si Title es null
            NodesJson = cleanNodes, 
            EdgesJson = cleanEdges
        };
    }
    private class GeminiGraphResponse
    {
        // Inicializamos con listas vacías para evitar nulos molestos
        public List<object> nodes { get; set; } = new List<object>();
        public List<object> edges { get; set; } = new List<object>();
    }
}