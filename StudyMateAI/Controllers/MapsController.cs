using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.UseCases.ConceptMaps.Commands;
using StudyMateAI.Application.UseCases.Mindmaps.Commands;

namespace StudyMateAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MapsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MapsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Endpoint 1: Generar Mapa Mental (Jerarquía)
    // POST: api/Maps/mindmap/generate
    [HttpPost("mindmap/generate")]
    public async Task<IActionResult> GenerateMindMap([FromBody] GenerateMindMapCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // Endpoint 2: Generar Mapa Conceptual (Red)
    // POST: api/Maps/conceptmap/generate
    [HttpPost("conceptmap/generate")]
    public async Task<IActionResult> GenerateConceptMap([FromBody] GenerateConceptMapCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}