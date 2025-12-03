using System.Text.Json.Serialization;

namespace StudyMateAI.Client.DTOs.Diagrams;

public class DiagramNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}