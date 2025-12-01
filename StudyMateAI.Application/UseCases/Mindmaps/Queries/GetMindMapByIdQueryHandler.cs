using MediatR;
using StudyMateAI.Application.DTOs.Mindmap;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Mindmaps.Queries;

internal class GetMindMapByIdQueryHandler : IRequestHandler<GetMindMapByIdQuery, GenerateMindMapResponseDto>
{
    private readonly IRepository<Mindmap> _repository;
    private readonly IDocumentRepository _documentRepository; // Para validar propiedad del doc

    public GetMindMapByIdQueryHandler(IRepository<Mindmap> repository, IDocumentRepository documentRepository)
    {
        _repository = repository;
        _documentRepository = documentRepository;
    }

    public async Task<GenerateMindMapResponseDto> Handle(GetMindMapByIdQuery request, CancellationToken cancellationToken)
    {
        var mindMap = await _repository.GetByIdAsync(request.MindMapId);
        
        if (mindMap == null) 
            throw new KeyNotFoundException($"Mapa Mental {request.MindMapId} no encontrado.");

        // Validación opcional de seguridad: verificar si el usuario es dueño del documento padre
        bool isOwner = await _documentRepository.UserOwnsDocumentAsync(mindMap.DocumentId, request.UserId);
        if (!isOwner) throw new UnauthorizedAccessException("No tienes permiso para ver este mapa.");

        return new GenerateMindMapResponseDto
        {
            MindMapId = mindMap.Id,
            DocumentId = mindMap.DocumentId,
            Title = mindMap.Title,
            NodesJson = mindMap.NodesJson,
            NodeCount = mindMap.NodeCount ?? 0,
            CreatedAt = mindMap.GeneratedAt ?? DateTime.MinValue
        };
    }
}