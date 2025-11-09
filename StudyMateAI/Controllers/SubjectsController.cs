using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.DTOs.Subject;
using StudyMateAI.Application.Services;
using StudyMateAI.Controllers.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyMateAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubjectsController : BaseApiController<CreateSubjectDto, UpdateSubjectDto, SubjectResponseDto>
    {
        private readonly ISubjectService _subjectService;

        public SubjectsController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        #region Implementación de métodos abstractos del BaseApiController

        protected override async Task<IEnumerable<SubjectResponseDto>> GetAllItemsAsync(int userId)
        {
            return await _subjectService.GetAllSubjectsAsync(userId);
        }

        protected override async Task<SubjectResponseDto?> GetItemByIdAsync(int userId, int itemId)
        {
            return await _subjectService.GetSubjectByIdAsync(userId, itemId);
        }

        protected override async Task<SubjectResponseDto> CreateItemAsync(int userId, CreateSubjectDto createDto)
        {
            return await _subjectService.CreateSubjectAsync(userId, createDto);
        }

        protected override async Task<SubjectResponseDto?> UpdateItemAsync(int userId, int itemId, UpdateSubjectDto updateDto)
        {
            return await _subjectService.UpdateSubjectAsync(userId, itemId, updateDto);
        }

        protected override async Task<bool> DeleteItemAsync(int userId, int itemId)
        {
            return await _subjectService.DeleteSubjectAsync(userId, itemId);
        }

        #endregion

        /// <summary>
        /// Obtener todas las materias del usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSubjects()
        {
            var userId = GetCurrentUserId();
            var subjects = await GetAllItemsAsync(userId);
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
            var subject = await GetItemByIdAsync(userId, id);

            if (subject == null)
                return NotFoundResponse("Materia no encontrada");

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
            var subject = await CreateItemAsync(userId, createDto);

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
            var subject = await UpdateItemAsync(userId, id, updateDto);

            if (subject == null)
                return NotFoundResponse("Materia no encontrada");

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
            var subject = await GetItemByIdAsync(userId, id);
            if (subject == null)
                return NotFoundResponse("Materia no encontrada");

            // Verificar si tiene documentos
            var hasDocuments = await _subjectService.CanDeleteSubjectAsync(id);
            
            if (hasDocuments && !force)
            {
                return BadRequestResponse("La materia contiene documentos. Use force=true para eliminarla de todas formas.");
            }

            var deleted = await DeleteItemAsync(userId, id);
            
            if (!deleted)
                return BadRequestResponse("No se pudo eliminar la materia");

            return SuccessResponse("Materia eliminada exitosamente");
        }

        /// <summary>
        /// Archivar/Desarchivar una materia
        /// </summary>
        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ToggleArchiveSubject(int id, [FromBody] ArchiveSubjectDto archiveDto)
        {
            var userId = GetCurrentUserId();
            
            var subject = await GetItemByIdAsync(userId, id);
            if (subject == null)
                return NotFoundResponse("Materia no encontrada");

            var updateDto = new UpdateSubjectDto
            {
                Name = subject.Name,
                Description = subject.Description,
                Color = subject.Color,
                Icon = subject.Icon,
                OrderIndex = subject.OrderIndex,
                IsArchived = archiveDto.IsArchived
            };

            var updatedSubject = await UpdateItemAsync(userId, id, updateDto);
            
            return Ok(updatedSubject);
        }
    }
}
