namespace StudyMateAI.Application.DTOs.ConceptMap;

public class GenerateConceptMapResponseDto
{
    public int ConceptMapId { get; set; }
    public required string Title { get; set; }
    public required object NodesJson { get; set; } 
    public required object EdgesJson { get; set; } 
}