using MediatR;
using StudyMateAI.Domain.Interfaces;

namespace StudyMateAI.Application.UseCases.Mindmaps.Commands;

internal class DeleteMindMapCommandHandler : IRequestHandler<DeleteMindMapCommand, bool>
{
    // 1. Variable privada con nombre completo
    private readonly IRepository<StudyMateAI.Domain.Entities.Mindmap> _repository;
    private readonly IDocumentRepository _docRepo;

    // 2. CONSTRUCTOR con nombre completo (Aquí estaba tu error)
    public DeleteMindMapCommandHandler(
        IRepository<StudyMateAI.Domain.Entities.Mindmap> repository, 
        IDocumentRepository docRepo)
    {
        _repository = repository;
        _docRepo = docRepo;
    }

    public async Task<bool> Handle(DeleteMindMapCommand request, CancellationToken ct)
    {
        var map = await _repository.GetByIdAsync(request.MindMapId);
        if (map == null) throw new KeyNotFoundException("Mapa Mental no encontrado.");

        bool isOwner = await _docRepo.UserOwnsDocumentAsync(map.DocumentId, request.UserId);
        if (!isOwner) throw new UnauthorizedAccessException("No tienes permiso.");

        await _repository.DeleteAsync(map.Id);
        return true;
    }
}