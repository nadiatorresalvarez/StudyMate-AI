using MediatR;
using StudyMateAI.Application.DTOs.Mindmap;

namespace StudyMateAI.Application.UseCases.Mindmaps.Queries;

public class GetMindMapByIdQuery : IRequest<GenerateMindMapResponseDto>
{
    public int MindMapId { get; set; }
    public int UserId { get; set; } // Para validar que sea dueño
}