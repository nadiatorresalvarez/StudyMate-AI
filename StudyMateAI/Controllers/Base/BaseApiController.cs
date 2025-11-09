using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudyMateAI.Controllers.Base
{
    /// <summary>
    /// Controlador base genérico para operaciones CRUD estándar
    /// </summary>
    /// <typeparam name="TCreateDto">DTO para crear</typeparam>
    /// <typeparam name="TUpdateDto">DTO para actualizar</typeparam>
    /// <typeparam name="TResponseDto">DTO de respuesta</typeparam>
    public abstract class BaseApiController<TCreateDto, TUpdateDto, TResponseDto> : ControllerBase
        where TCreateDto : class
        where TUpdateDto : class
        where TResponseDto : class
    {
        /// <summary>
        /// Obtiene el ID del usuario actual desde el token JWT
        /// </summary>
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }
            return int.Parse(userIdClaim);
        }

        /// <summary>
        /// Obtiene el email del usuario actual desde el token JWT
        /// </summary>
        protected string? GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Obtiene el nombre del usuario actual desde el token JWT
        /// </summary>
        protected string? GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Método abstracto para obtener todos los elementos
        /// </summary>
        protected abstract Task<IEnumerable<TResponseDto>> GetAllItemsAsync(int userId);

        /// <summary>
        /// Método abstracto para obtener un elemento por ID
        /// </summary>
        protected abstract Task<TResponseDto?> GetItemByIdAsync(int userId, int itemId);

        /// <summary>
        /// Método abstracto para crear un nuevo elemento
        /// </summary>
        protected abstract Task<TResponseDto> CreateItemAsync(int userId, TCreateDto createDto);

        /// <summary>
        /// Método abstracto para actualizar un elemento
        /// </summary>
        protected abstract Task<TResponseDto?> UpdateItemAsync(int userId, int itemId, TUpdateDto updateDto);

        /// <summary>
        /// Método abstracto para eliminar un elemento
        /// </summary>
        protected abstract Task<bool> DeleteItemAsync(int userId, int itemId);

        /// <summary>
        /// Respuesta estándar para recursos no encontrados
        /// </summary>
        protected IActionResult NotFoundResponse(string message = "Recurso no encontrado")
        {
            return NotFound(new { message });
        }

        /// <summary>
        /// Respuesta estándar para solicitudes incorrectas
        /// </summary>
        protected IActionResult BadRequestResponse(string message)
        {
            return BadRequest(new { message });
        }

        /// <summary>
        /// Respuesta estándar para operaciones exitosas
        /// </summary>
        protected IActionResult SuccessResponse(string message, object? data = null)
        {
            if (data == null)
            {
                return Ok(new { message });
            }
            return Ok(new { message, data });
        }
    }
}

