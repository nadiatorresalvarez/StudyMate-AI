using MediatR;
using StudyMateAI.Application.DTOs.ConceptMap;
using StudyMateAI.Domain.Entities;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.ConceptMaps.Queries;

internal class GetConceptMapByIdQueryHandler : IRequestHandler<GetConceptMapByIdQuery, GenerateConceptMapResponseDto>
{
    private readonly IRepository<Conceptmap> _repository;
    private readonly IDocumentRepository _docRepo;

    public GetConceptMapByIdQueryHandler(IRepository<Conceptmap> repository, IDocumentRepository docRepo)
    {
        _repository = repository;
        _docRepo = docRepo;
    }

    public async Task<GenerateConceptMapResponseDto> Handle(GetConceptMapByIdQuery request, CancellationToken ct)
    {
        var map = await _repository.GetByIdAsync(request.ConceptMapId);
        if (map == null) throw new KeyNotFoundException("Mapa Conceptual no encontrado");

        bool isOwner = await _docRepo.UserOwnsDocumentAsync(map.DocumentId, request.UserId);
        if (!isOwner) throw new UnauthorizedAccessException("Sin permiso");

        return new GenerateConceptMapResponseDto
        {
            ConceptMapId = map.Id,
            NodesJson = map.NodesJson,
            EdgesJson = map.EdgesJson,
            Title = map.Title
        };
    }
}