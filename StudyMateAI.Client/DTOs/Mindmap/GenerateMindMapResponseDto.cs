namespace StudyMateAI.Client.DTOs.Mindmap;

public class GenerateMindMapResponseDto
{
    public int MindMapId { get; set; }
    public string NodesJson { get; set; } = string.Empty;
}