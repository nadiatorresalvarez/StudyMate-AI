using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.DTOs.Summary;
using StudyMateAI.Application.UseCases.Summaries.Commands;

namespace StudyMateAI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SummariesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SummariesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate/{documentId:int}")]
    [ProducesResponseType(typeof(GenerateBriefSummaryResponseDto), 200)]
    public async Task<IActionResult> GenerateBriefSummary(int documentId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new GenerateBriefSummaryCommand
        {
            UserId = int.Parse(userIdClaim),
            DocumentId = documentId
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(new
            {
                summaryText = result.SummaryText,
                documentId = result.DocumentId,
                createdAt = result.CreatedAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }   // <--- ESTE } CIERRA GenerateBriefSummary

    [HttpPost("generate-detailed/{documentId:int}")]
    [ProducesResponseType(typeof(GenerateBriefSummaryResponseDto), 200)]
    public async Task<IActionResult> GenerateDetailedSummary(int documentId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new GenerateDetailedSummaryCommand
        {
            UserId = int.Parse(userIdClaim),
            DocumentId = documentId
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(new
            {
                summaryText = result.SummaryText,
                documentId = result.DocumentId,
                createdAt = result.CreatedAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("generate-key-concepts/{documentId:int}")]
    [ProducesResponseType(typeof(GenerateBriefSummaryResponseDto), 200)]
    public async Task<IActionResult> GenerateKeyConcepts(int documentId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new GenerateKeyConceptsCommand
        {
            UserId = int.Parse(userIdClaim),
            DocumentId = documentId
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(new
            {
                summaryText = result.SummaryText,
                documentId = result.DocumentId,
                createdAt = result.CreatedAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}