using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Application.Services;
using System.Security.Claims;

namespace StudyMateAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectsController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        #region Helper Methods

        /// <summary>
        /// Obtiene el ID del usuario actual desde el token JWT
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }
            return int.Parse(userIdClaim);
        }

        #endregion

        /// <summary>
        /// Obtener todas las materias del usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSubjects()
        {
            var userId = GetCurrentUserId();
            var subjects = await _subjectService.GetAllSubjectsAsync(userId);
            return Ok(subjects);
        }

        /// <summary>
        /// Obtener solo las materias activas (no archivadas)
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSubjects()
        {
            var userId = GetCurrentUserId();
            var subjects = await _subjectService.GetActiveSubjectsAsync(userId);
            return Ok(subjects);
        }

        /// <summary>
        /// Obtener una materia específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            var userId = GetCurrentUserId();
            var subject = await _subjectService.GetSubjectByIdAsync(userId, id);

            if (subject == null)
                return NotFound(new { message = "Materia no encontrada" });

            return Ok(subject);
        }

        /// <summary>
        /// Crear una nueva materia
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var subject = await _subjectService.CreateSubjectAsync(userId, createDto);

            return CreatedAtAction(nameof(GetSubjectById), new { id = subject.Id }, subject);
        }

        /// <summary>
        /// Actualizar una materia existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var subject = await _subjectService.UpdateSubjectAsync(userId, id, updateDto);

            if (subject == null)
                return NotFound(new { message = "Materia no encontrada" });

            return Ok(subject);
        }

        /// <summary>
        /// Eliminar una materia (con validación si contiene documentos)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id, [FromQuery] bool force = false)
        {
            var userId = GetCurrentUserId();

            // Verificar si la materia existe y pertenece al usuario
            var subject = await _subjectService.GetSubjectByIdAsync(userId, id);
            if (subject == null)
                return NotFound(new { message = "Materia no encontrada" });

            // Verificar si tiene documentos
            var hasDocuments = await _subjectService.CanDeleteSubjectAsync(id);
            
            if (hasDocuments && !force)
            {
                return BadRequest(new { message = "La materia contiene documentos. Use force=true para eliminarla de todas formas." });
            }

            var deleted = await _subjectService.DeleteSubjectAsync(userId, id);
            
            if (!deleted)
                return BadRequest(new { message = "No se pudo eliminar la materia" });

            return Ok(new { message = "Materia eliminada exitosamente" });
        }

        /// <summary>
        /// Archivar/Desarchivar una materia
        /// </summary>
        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ToggleArchiveSubject(int id, [FromBody] ArchiveSubjectDto archiveDto)
        {
            var userId = GetCurrentUserId();
            
            var subject = await _subjectService.GetSubjectByIdAsync(userId, id);
            if (subject == null)
                return NotFound(new { message = "Materia no encontrada" });

            var updateDto = new UpdateSubjectDto
            {
                Name = subject.Name,
                Description = subject.Description,
                Color = subject.Color,
                Icon = subject.Icon,
                OrderIndex = subject.OrderIndex,
                IsArchived = archiveDto.IsArchived
            };

            var updatedSubject = await _subjectService.UpdateSubjectAsync(userId, id, updateDto);
            
            return Ok(updatedSubject);
        }
    }
}
