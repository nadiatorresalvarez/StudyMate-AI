namespace StudyMateAI.Application.DTOs.Mindmap;

public class GenerateMindMapResponseDto
{
    public int MindMapId { get; set; }
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string NodesJson { get; set; } = string.Empty; // Coincide con tu entidad
    public int NodeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}