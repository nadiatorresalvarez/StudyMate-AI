using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.UseCases.Flashcards.Commands;
using StudyMateAI.Application.DTOs.Flashcards;

namespace StudyMateAI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FlashcardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FlashcardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate/{documentId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<GeneratedFlashcardDto>), 200)]
    public async Task<IActionResult> GenerateFromDocument(int documentId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new GenerateFlashcardsFromDocumentCommand
        {
            UserId = int.Parse(userIdClaim),
            DocumentId = documentId
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
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

    [HttpPost("{documentId:int}")]
    [ProducesResponseType(typeof(FlashcardResponseDto), 200)]
    public async Task<IActionResult> CreateManual(int documentId, [FromBody] CreateFlashcardRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new CreateFlashcardCommand(int.Parse(userIdClaim), documentId, request);

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
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

    [HttpPut("{flashcardId:int}")]
    [ProducesResponseType(typeof(FlashcardResponseDto), 200)]
    public async Task<IActionResult> UpdateManual(int flashcardId, [FromBody] UpdateFlashcardRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new UpdateFlashcardCommand(int.Parse(userIdClaim), flashcardId, request);

        try
        {
            var result = await _mediator.Send(command);
            if (result == null)
                return NotFound(new { message = "Flashcard no encontrada" });

            return Ok(result);
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

    [HttpDelete("{flashcardId:int}")]
    public async Task<IActionResult> DeleteManual(int flashcardId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new DeleteFlashcardCommand(int.Parse(userIdClaim), flashcardId);

        try
        {
            var deleted = await _mediator.Send(command);
            if (!deleted)
                return NotFound(new { message = "Flashcard no encontrada" });

            return Ok(new { message = "Flashcard eliminada correctamente" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("review/{flashcardId:int}")]
    public async Task<IActionResult> Review(int flashcardId, [FromBody] ReviewFlashcardRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new ReviewFlashcardCommand(int.Parse(userIdClaim), flashcardId, request.Quality);

        try
        {
            var success = await _mediator.Send(command);
            if (!success)
                return NotFound(new { message = "Flashcard no encontrada" });

            return Ok(new { message = "Review registrada correctamente" });
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
