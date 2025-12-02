using MediatR;

namespace StudyMateAI.Application.UseCases.Mindmaps.Commands;

public class DeleteMindMapCommand : IRequest<bool>
{
    public int MindMapId { get; set; }
    public int UserId { get; set; } // Para seguridad (solo el dueño borra)
}