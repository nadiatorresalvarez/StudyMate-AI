using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.Common.Exceptions;
using StudyMateAI.Application.DTOs.Summary;
using StudyMateAI.Application.UseCases.Summaries.Commands;
using StudyMateAI.Application.UseCases.Summaries.Queries;

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
                summaryId = result.SummaryId,
                summaryText = result.SummaryText,
                documentId = result.DocumentId,
                createdAt = result.CreatedAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
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
                summaryId = result.SummaryId,
                summaryText = result.SummaryText,
                documentId = result.DocumentId,
                createdAt = result.CreatedAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
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
                summaryId = result.SummaryId,
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

    [HttpGet("{resumenId:int}/download")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public async Task<IActionResult> DownloadResumenWord(int resumenId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var query = new GetResumenWordQuery
        {
            UserId = int.Parse(userIdClaim),
            ResumenId = resumenId
        };

        try
        {
            var fileDto = await _mediator.Send(query);
            return File(fileDto.Content, fileDto.ContentType, fileDto.FileName);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar el reporte.", details = ex.Message });
        }
    }
}