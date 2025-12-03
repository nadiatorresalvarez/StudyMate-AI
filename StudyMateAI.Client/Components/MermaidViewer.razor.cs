using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StudyMateAI.Client.DTOs.Diagrams;
using Microsoft.JSInterop;
using System;

namespace StudyMateAI.Client.Components
{
    public partial class MermaidViewer : ComponentBase
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter]
        public List<DiagramNode>? Nodes { get; set; }

        [Parameter]
        public List<DiagramEdge>? Edges { get; set; }

        private string _elementId = $"mermaid-{Guid.NewGuid()}";

        protected override async Task OnParametersSetAsync()
        {
            await RenderMap();
        }

        private async Task RenderMap()
        {
            string mermaidCode;

            if (Nodes == null || Nodes.Count == 0)
            {
                mermaidCode = "graph TD;\n  A[No hay datos para mostrar el mapa];";
            }
            else
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("graph TD;");

                    foreach (var node in Nodes)
                    {
                        sb.AppendLine($"    {node.Id}[\"{node.Label.Replace("\"", "'")}\"];");
                    }
                    sb.AppendLine();

                    if (Edges != null)
                    {
                        foreach (var edge in Edges)
                        {
                            if (!string.IsNullOrEmpty(edge.Label))
                            {
                                sb.AppendLine($"    {edge.Source} -- \"{edge.Label}\" --> {edge.Target};");
                            }
                            else
                            {
                                sb.AppendLine($"    {edge.Source} --> {edge.Target};");
                            }
                        }
                    }
                    mermaidCode = sb.ToString();
                }
                catch (Exception ex)
                {
                    mermaidCode = $"graph TD;\n  Error[\"Error: {ex.Message.Replace("\"", "'")}\"]";
                }
            }
            await JSRuntime.InvokeVoidAsync("renderMermaid", _elementId, mermaidCode);
        }
    }
}