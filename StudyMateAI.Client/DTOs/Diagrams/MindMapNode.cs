using System.Text.Json.Serialization;

namespace StudyMateAI.Client.DTOs.Diagrams;

public class MindMapNode
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("children")]
    public List<MindMapNode>? Children { get; set; }
}