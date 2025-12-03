namespace StudyMateAI.Client.DTOs.ConceptMap;

public class GenerateConceptMapResponseDto
{
    public int ConceptMapId { get; set; }
    public string NodesJson { get; set; } = string.Empty;
    public string EdgesJson { get; set; } = string.Empty;
}