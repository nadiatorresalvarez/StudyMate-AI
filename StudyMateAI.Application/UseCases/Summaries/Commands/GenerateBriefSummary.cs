using MediatR;
using StudyMateAI.Application.DTOs.Summary;

namespace StudyMateAI.Application.UseCases.Summaries.Commands;

public class GenerateBriefSummaryCommand : IRequest<GenerateBriefSummaryResponseDto>
{
    public int UserId { get; set; }
    public int DocumentId { get; set; }
}
