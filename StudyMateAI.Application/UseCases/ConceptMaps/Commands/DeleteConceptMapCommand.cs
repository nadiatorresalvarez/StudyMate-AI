using MediatR;

namespace StudyMateAI.Application.UseCases.ConceptMaps.Commands;

public class DeleteConceptMapCommand : IRequest<bool>
{
    public int ConceptMapId { get; set; }
    public int UserId { get; set; }
}