using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.UseCases.ConceptMaps.Commands;
using StudyMateAI.Application.UseCases.ConceptMaps.Queries;
using StudyMateAI.Application.UseCases.Mindmaps.Commands;
using StudyMateAI.Application.UseCases.Mindmaps.Queries;

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
    
    // Endpoint para obtener un MindMap por ID
    [HttpGet("mindmap/{id}")]
    public async Task<IActionResult> GetMindMap(int id)
    {
        // Simulación de UserId (en producción lo sacas del Token)
        int userId = 1; 

        var query = new GetMindMapByIdQuery { MindMapId = id, UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    // Endpoint para obtener un ConceptMap por ID
    [HttpGet("conceptmap/{id}")]
    public async Task<IActionResult> GetConceptMap(int id)
    {
        int userId = 1; // Sacar del token real
        var query = new GetConceptMapByIdQuery { ConceptMapId = id, UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpDelete("mindmap/{id}")]
    public async Task<IActionResult> DeleteMindMap(int id)
    {
        int userId = 1; // Sacar del Token en producción
        var command = new DeleteMindMapCommand { MindMapId = id, UserId = userId };
    
        await _mediator.Send(command);
        return NoContent(); // 204 No Content es el estándar para borrados exitosos
    }
    
    [HttpDelete("conceptmap/{id}")]
    public async Task<IActionResult> DeleteConceptMap(int id)
    {
        int userId = 1; // Sacar del token
        var command = new DeleteConceptMapCommand { ConceptMapId = id, UserId = userId };
    
        await _mediator.Send(command);
        return NoContent();
    }
}