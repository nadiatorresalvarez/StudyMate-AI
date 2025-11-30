using System;

namespace StudyMateAI.Application.DTOs.Summary;

public class GenerateBriefSummaryResponseDto
{
    public int DocumentId { get; set; }
    public int SummaryId { get; set; }
    public string SummaryText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
