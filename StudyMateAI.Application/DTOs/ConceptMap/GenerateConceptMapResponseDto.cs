namespace StudyMateAI.Application.DTOs.ConceptMap;

public class GenerateConceptMapResponseDto 
{
    public int ConceptMapId { get; set; }
    public string NodesJson { get; set; }
    public string EdgesJson { get; set; }
    public string Title { get; set; } = string.Empty;
}