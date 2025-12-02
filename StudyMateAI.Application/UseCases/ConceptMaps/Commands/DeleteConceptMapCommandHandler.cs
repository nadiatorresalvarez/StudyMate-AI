using MediatR;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.ConceptMaps.Commands;

internal class DeleteConceptMapCommandHandler : IRequestHandler<DeleteConceptMapCommand, bool>
{
    // 👇 CORRECCIÓN: Usamos el nombre completo aquí también para evitar errores
    private readonly IRepository<StudyMateAI.Domain.Entities.Conceptmap> _repository;
    private readonly IDocumentRepository _docRepo;

    public DeleteConceptMapCommandHandler(
        IRepository<StudyMateAI.Domain.Entities.Conceptmap> repository, 
        IDocumentRepository docRepo)
    {
        _repository = repository;
        _docRepo = docRepo;
    }

    public async Task<bool> Handle(DeleteConceptMapCommand request, CancellationToken ct)
    {
        var map = await _repository.GetByIdAsync(request.ConceptMapId);
        
        if (map == null) throw new KeyNotFoundException("Mapa Conceptual no encontrado.");

        bool isOwner = await _docRepo.UserOwnsDocumentAsync(map.DocumentId, request.UserId);
        if (!isOwner) throw new UnauthorizedAccessException("Sin permiso.");

        await _repository.DeleteAsync(map.Id);
        return true;
    }
}