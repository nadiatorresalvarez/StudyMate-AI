using MediatR;
using StudyMateAI.Application.DTOs.ConceptMap;

namespace StudyMateAI.Application.UseCases.ConceptMaps.Queries;

public class GetConceptMapByIdQuery : IRequest<GenerateConceptMapResponseDto>
{
    public int ConceptMapId { get; set; }
    public int UserId { get; set; }
}