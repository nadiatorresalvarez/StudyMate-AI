using MediatR;
using StudyMateAI.Application.DTOs.Summary;

namespace StudyMateAI.Application.UseCases.Summaries.Commands;

public class GenerateKeyConceptsCommand : IRequest<GenerateBriefSummaryResponseDto>
{
    public int UserId { get; set; }
    public int DocumentId { get; set; }
}
