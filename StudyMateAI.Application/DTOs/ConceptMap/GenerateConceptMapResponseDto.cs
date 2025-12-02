namespace StudyMateAI.Application.DTOs.ConceptMap;

public class GenerateConceptMapResponseDto
{
    public int ConceptMapId { get; set; }
    public string Title { get; set; }
    public object NodesJson { get; set; } 
    public object EdgesJson { get; set; } 
}