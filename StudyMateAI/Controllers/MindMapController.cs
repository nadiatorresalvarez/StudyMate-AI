using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.UseCases.Mindmaps.Commands;

namespace StudyMateAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MindMapController : ControllerBase
{
    private readonly IMediator _mediator;

    public MindMapController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Genera un Mapa Mental a partir de un documento existente.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateMindMapCommand command)
    {
        // Opcional: Si estás logueado, forzamos el ID del usuario del token
        // var userIdClaim = User.FindFirst("uid") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        // if (userIdClaim != null) command.UserId = int.Parse(userIdClaim.Value);

        var result = await _mediator.Send(command);
        return Ok(result); // Devuelve 200 OK con el JSON del mapa
    }
}