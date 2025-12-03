using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.Json;
using StudyMateAI.Client.DTOs.Diagrams;
using Microsoft.JSInterop;

namespace StudyMateAI.Client.Components
{
    public partial class MermaidViewer : ComponentBase
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter]
        public string? NodesJson { get; set; }

        [Parameter]
        public string? EdgesJson { get; set; }

        private string _elementId = $"mermaid-{Guid.NewGuid()}";

        // Usamos OnAfterRenderAsync para asegurar que el div exista antes de llamar a JS
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Solo queremos renderizar si es la primera vez que se muestra
            // o si los datos han cambiado (lo que fuerza una nueva renderización)
            if (firstRender) 
            {
                 await RenderMap();
            }
        }
        
        // Hacemos este método público para poder volver a llamarlo si es necesario
        public async Task RenderMap()
        {
            string mermaidCode;

            if (!string.IsNullOrWhiteSpace(EdgesJson))
            {
                mermaidCode = GenerateConceptMapCode();
            }
            else if (!string.IsNullOrWhiteSpace(NodesJson))
            {
                mermaidCode = GenerateMindMapCode();
            }
            else
            {
                return; // No renderizamos nada si no hay datos
            }
            
            // Pequeña espera para asegurar que el DOM esté listo
            await Task.Delay(50);
            await JSRuntime.InvokeVoidAsync("renderMermaid", _elementId, mermaidCode);
        }

        private string GenerateConceptMapCode()
        {
            try
            {
                var sb = new StringBuilder("graph LR;"); // Left-to-Right
                var nodes = JsonSerializer.Deserialize<List<DiagramNode>>(NodesJson!);
                var edges = JsonSerializer.Deserialize<List<DiagramEdge>>(EdgesJson!);
                if (nodes == null) return string.Empty;
                foreach (var node in nodes) sb.AppendLine($"    {node.Id}[\"{node.Label.Replace("\"", "'")}\"];");
                if (edges != null)
                {
                    foreach (var edge in edges)
                    {
                        if (!string.IsNullOrEmpty(edge.Label)) sb.AppendLine($"    {edge.Source} -->|\"{edge.Label.Replace("\"", "'")}\"| {edge.Target};");
                        else sb.AppendLine($"    {edge.Source} --> {edge.Target};");
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex) { return $"graph TD; Error[\"Error: {ex.Message}\"];"; }
        }

        private string GenerateMindMapCode()
        {
            try
            {
                var sb = new StringBuilder("graph LR;"); // Left-to-Right
                var rootNode = JsonSerializer.Deserialize<MindMapNode>(NodesJson!);
                if (rootNode == null) return string.Empty;
                void Traverse(MindMapNode parent, string parentId)
                {
                    if (parent.Children == null) return;
                    foreach (var child in parent.Children)
                    {
                        var childId = $"id_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                        sb.AppendLine($"    {childId}[\"{child.Label.Replace("\"", "'")}\"];");
                        sb.AppendLine($"    {parentId} --> {childId};");
                        Traverse(child, childId);
                    }
                }
                var rootId = "root";
                sb.AppendLine($"{rootId}[\"{rootNode.Label.Replace("\"", "'")}\"];");
                Traverse(rootNode, rootId);
                return sb.ToString();
            }
            catch (Exception ex) { return $"graph TD; Error[\"Error: {ex.Message}\"];"; }
        }
    }
}