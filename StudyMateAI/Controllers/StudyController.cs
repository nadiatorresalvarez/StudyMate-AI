using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.DTOs.Flashcards;
using StudyMateAI.Application.UseCases.Flashcards.Queries;

namespace StudyMateAI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudyController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("next")]
    [ProducesResponseType(typeof(NextStudyFlashcardDto), 200)]
    public async Task<IActionResult> GetNext()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var userId = int.Parse(userIdClaim);
        var query = new GetNextStudyFlashcardQuery(userId);

        var result = await _mediator.Send(query);

        if (result == null)
        {
            return Ok(null); // No hay tarjetas pendientes hoy
        }

        return Ok(result);
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(StudyDashboardDto), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var userId = int.Parse(userIdClaim);
        var query = new GetStudyDashboardQuery(userId);

        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
