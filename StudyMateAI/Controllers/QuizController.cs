using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.DTOs.Quizzes;
using StudyMateAI.Application.UseCases.Quizzes.Commands;
using StudyMateAI.Application.UseCases.Quizzes.Queries;

namespace StudyMateAI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuizController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate/{documentId:int}")]
    [ProducesResponseType(typeof(GenerateQuizResponseDto), 200)]
    public async Task<IActionResult> GenerateFromDocument(int documentId, [FromBody] GenerateQuizRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new GenerateQuizFromDocumentCommand
        {
            UserId = int.Parse(userIdClaim),
            DocumentId = documentId,
            QuestionCount = request.QuestionCount,
            Difficulty = request.Difficulty
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

    [HttpGet("{quizId:int}/for-attempt")]
    [ProducesResponseType(typeof(QuizForAttemptDto), 200)]
    public async Task<IActionResult> GetForAttempt(int quizId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var query = new GetQuizForAttemptQuery(int.Parse(userIdClaim), quizId);

        try
        {
            var result = await _mediator.Send(query);
            if (result == null)
                return NotFound(new { message = "Quiz no encontrado" });

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("{quizId:int}/attempts")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> SubmitAttempt(int quizId, [FromBody] SubmitQuizAttemptRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new SubmitQuizAttemptCommand(int.Parse(userIdClaim), quizId, request);

        try
        {
            var attemptId = await _mediator.Send(command);
            return Ok(new { attemptId });
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

    [HttpPost("attempts/{attemptId:int}/evaluate")]
    [ProducesResponseType(typeof(QuizAttemptResultDto), 200)]
    public async Task<IActionResult> EvaluateAttempt(int attemptId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new EvaluateQuizAttemptCommand(int.Parse(userIdClaim), attemptId);

        try
        {
            var result = await _mediator.Send(command);
            if (result == null)
                return NotFound(new { message = "Intento no encontrado" });

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

    [HttpGet("attempts/{attemptId:int}")]
    [ProducesResponseType(typeof(QuizAttemptResultDto), 200)]
    public async Task<IActionResult> GetAttemptResult(int attemptId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var query = new GetQuizAttemptResultQuery(int.Parse(userIdClaim), attemptId);

        try
        {
            var result = await _mediator.Send(query);
            if (result == null)
                return NotFound(new { message = "Intento no encontrado" });

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

    [HttpGet("attempts/history")]
    [ProducesResponseType(typeof(QuizHistoryResponseDto), 200)]
    public async Task<IActionResult> GetHistory([FromQuery] int? documentId, [FromQuery] int? quizId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var query = new GetQuizHistoryQuery(int.Parse(userIdClaim), documentId, quizId);

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
