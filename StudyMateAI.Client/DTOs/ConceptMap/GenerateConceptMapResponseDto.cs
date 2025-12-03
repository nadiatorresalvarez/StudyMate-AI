using System.Text.Json.Serialization;
using StudyMateAI.Client.DTOs.Diagrams;

namespace StudyMateAI.Client.DTOs.ConceptMap
{
    public class GenerateConceptMapResponseDto
    {
        public int ConceptMapId { get; set; }
        
        [JsonPropertyName("nodesJson")]
        public List<DiagramNode> Nodes { get; set; } = new(); 
        
        [JsonPropertyName("edgesJson")]
        public List<DiagramEdge> Edges { get; set; } = new();
    }
}