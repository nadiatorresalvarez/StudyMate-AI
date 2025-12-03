using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        
        // Variable para guardar el código generado
        private string _mermaidCode = string.Empty;

        // OnParametersSetAsync es ideal para PROCESAR datos
        protected override Task OnParametersSetAsync()
        {
            GenerateMermaidCode();
            return base.OnParametersSetAsync();
        }

        // OnAfterRenderAsync es ideal para INTERACTUAR con JS
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Solo llamamos a JS si tenemos código que renderizar
            if (!string.IsNullOrEmpty(_mermaidCode))
            {
                await JSRuntime.InvokeVoidAsync("renderMermaid", _elementId, _mermaidCode);
            }
        }

        private void GenerateMermaidCode()
        {
            if (!string.IsNullOrWhiteSpace(EdgesJson))
            {
                _mermaidCode = GenerateConceptMapCode();
            }
            else if (!string.IsNullOrWhiteSpace(NodesJson))
            {
                _mermaidCode = GenerateMindMapCode();
            }
            else
            {
                _mermaidCode = string.Empty;
            }
        }

        private string GenerateConceptMapCode()
        {
            try
            {
                var nodes = JsonSerializer.Deserialize<List<DiagramNode>>(NodesJson!);
                var edges = JsonSerializer.Deserialize<List<DiagramEdge>>(EdgesJson!);
                if (nodes == null) return string.Empty;
                var sb = new StringBuilder("graph TD;");
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
            catch (Exception ex) { return $"graph TD;\n  Error[\"Error: {ex.Message.Replace("\"", "'")}\"]"; }
        }

        private string GenerateMindMapCode()
        {
            try
            {
                var rootNode = JsonSerializer.Deserialize<MindMapNode>(NodesJson!);
                if (rootNode == null) return string.Empty;
                var sb = new StringBuilder("graph TD;");
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
            catch (Exception ex) { return $"graph TD;\n  Error[\"Error: {ex.Message.Replace("\"", "'")}\"]"; }
        }
    }
}