using MediatR;
using StudyMateAI.Application.DTOs.ConceptMap;

namespace StudyMateAI.Application.UseCases.ConceptMaps.Commands;

public class GenerateConceptMapCommand : IRequest<GenerateConceptMapResponseDto>
{
    public int DocumentId { get; set; }
    public int UserId { get; set; }
}