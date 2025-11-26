using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Application.Features.Subjects.Commands.CreateSubject;
using StudyMateAI.Application.Features.Subjects.Commands.UpdateSubject;
using StudyMateAI.Application.Features.Subjects.Commands.DeleteSubject;
using StudyMateAI.Application.Features.Subjects.Queries.GetAllSubjects;
using StudyMateAI.Application.Features.Subjects.Queries.GetActiveSubjects;
using StudyMateAI.Application.Features.Subjects.Queries.GetSubjectById;
using StudyMateAI.Application.Features.Subjects.Queries.CanDeleteSubject;
using System.Security.Claims;

namespace StudyMateAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubjectsController(IMediator mediator)
        {
            _mediator = mediator;
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
            var query = new GetAllSubjectsQuery(userId);
            var subjects = await _mediator.Send(query);
            return Ok(subjects);
        }

        /// <summary>
        /// Obtener solo las materias activas (no archivadas)
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSubjects()
        {
            var userId = GetCurrentUserId();
            var query = new GetActiveSubjectsQuery(userId);
            var subjects = await _mediator.Send(query);
            return Ok(subjects);
        }

        /// <summary>
        /// Obtener una materia específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            var userId = GetCurrentUserId();
            var query = new GetSubjectByIdQuery(userId, id);
            var subject = await _mediator.Send(query);

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
            var command = new CreateSubjectCommand(userId, createDto);
            var subject = await _mediator.Send(command);

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
            var command = new UpdateSubjectCommand(userId, id, updateDto);
            var subject = await _mediator.Send(command);

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
            var getQuery = new GetSubjectByIdQuery(userId, id);
            var subject = await _mediator.Send(getQuery);
            
            if (subject == null)
                return NotFound(new { message = "Materia no encontrada" });

            // Verificar si tiene documentos
            var canDeleteQuery = new CanDeleteSubjectQuery(id);
            var hasDocuments = await _mediator.Send(canDeleteQuery);
            
            if (hasDocuments && !force)
            {
                return BadRequest(new { message = "La materia contiene documentos. Use force=true para eliminarla de todas formas." });
            }

            var deleteCommand = new DeleteSubjectCommand(userId, id);
            var deleted = await _mediator.Send(deleteCommand);
            
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
            
            var getQuery = new GetSubjectByIdQuery(userId, id);
            var subject = await _mediator.Send(getQuery);
            
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

            var updateCommand = new UpdateSubjectCommand(userId, id, updateDto);
            var updatedSubject = await _mediator.Send(updateCommand);
            
            return Ok(updatedSubject);
        }
    }
}
