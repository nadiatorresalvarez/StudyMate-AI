using MediatR;
using StudyMateAI.Application.DTOs.Mindmap;

namespace StudyMateAI.Application.UseCases.Mindmaps.Commands;

public class GenerateMindMapCommand : IRequest<GenerateMindMapResponseDto>
{
    public int DocumentId { get; set; }
    public int UserId { get; set; }
    // Opcional: Si quieres personalizar el color o estilo desde el frontend
    public string ColorScheme { get; set; } = "default"; 
}