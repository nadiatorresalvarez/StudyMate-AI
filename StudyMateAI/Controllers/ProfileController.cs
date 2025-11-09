using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.UseCases.Users.Commands;
using StudyMateAI.Application.UseCases.Users.Queries;
using StudyMateAI.DTOs.Request;

namespace StudyMateAI.Controllers;

[ApiController]
[Route("api/[controller]")] 
[Authorize] 
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/profile
    [HttpGet]
    public async Task<IActionResult> GetUserProfile()
    {
        // Obtenemos el ID del usuario desde el token JWT que viene en la petición.
        // Esto es mucho más seguro que pasarlo por la URL.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var query = new GetUserProfileQuery { UserId = int.Parse(userId) };

        // Enviamos el query a MediatR, que encontrará y ejecutará el handler correspondiente.
        var userProfileDto = await _mediator.Send(query);

        return Ok(userProfileDto);
    }

    // PUT: api/profile
    [HttpPut]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        var command = new UpdateUserProfileCommand
        {
            UserId = int.Parse(userId),
            Name = request.Name,
            EducationLevel = request.EducationLevel
        };

        // Enviamos el comando a MediatR para que lo procese.
        await _mediator.Send(command);

        return Ok();
    }
}