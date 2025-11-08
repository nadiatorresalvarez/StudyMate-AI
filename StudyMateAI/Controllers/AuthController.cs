using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.DTOs.Auth;
using StudyMateAI.Application.UseCases.Auth;
using System.Threading.Tasks;

// 1. Namespace correcto
namespace StudyMateAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GoogleAuthUseCase _googleAuthUseCase;

        public AuthController(GoogleAuthUseCase googleAuthUseCase)
        {
            _googleAuthUseCase = googleAuthUseCase;
        }

        /// <summary>
        /// Inicia sesión o registra un usuario usando un ID Token de Google. (TAREA RF-001.1)
        /// </summary>
        [HttpPost("google-login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)] // Respuesta OK
        [ProducesResponseType(400)] // BadRequest
        [ProducesResponseType(401)] // Unauthorized
        public async Task<IActionResult> GoogleLogin([FromBody] AuthRequestDto request)
        {
            if (string.IsNullOrEmpty(request.GoogleIdToken))
            {
                return BadRequest("Token de Google es requerido.");
            }

            try
            {
                // El UseCase hace toda la magia
                var response = await _googleAuthUseCase.ExecuteAsync(request);
                return Ok(response);
            }
            catch (Google.Apis.Auth.InvalidJwtException ex)
            {
                // Token inválido
                return Unauthorized($"Token de Google no válido: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                // Error interno
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}