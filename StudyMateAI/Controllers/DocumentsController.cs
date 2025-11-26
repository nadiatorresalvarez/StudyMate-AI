using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Application.Services;
using System.Security.Claims;

namespace StudyMateAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
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
        /// Obtener todos los documentos del usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            var userId = GetCurrentUserId();
            var documents = await _documentService.GetAllDocumentsAsync(userId);
            return Ok(documents);
        }

        /// <summary>
        /// Obtener un documento específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            var userId = GetCurrentUserId();
            var document = await _documentService.GetDocumentByIdAsync(userId, id);

            if (document == null)
                return NotFound(new { message = "Documento no encontrado" });

            return Ok(document);
        }

        /// <summary>
        /// Obtener todos los documentos de una materia específica
        /// </summary>
        [HttpGet("subject/{subjectId}")]
        public async Task<IActionResult> GetDocumentsBySubject(int subjectId)
        {
            var userId = GetCurrentUserId();
            var documents = await _documentService.GetDocumentsBySubjectAsync(userId, subjectId);
            return Ok(documents);
        }

        /// <summary>
        /// Obtener documentos por estado de procesamiento (Pending, Completed, Failed)
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetDocumentsByStatus(string status)
        {
            var userId = GetCurrentUserId();
            var documents = await _documentService.GetDocumentsByStatusAsync(userId, status);
            return Ok(documents);
        }

        /// <summary>
        /// Crear un nuevo documento
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            
            try
            {
                var document = await _documentService.CreateDocumentAsync(userId, createDto);
                return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar un documento existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateDocumentDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var document = await _documentService.UpdateDocumentAsync(userId, id, updateDto);

            if (document == null)
                return NotFound(new { message = "Documento no encontrado o no autorizado" });

            return Ok(document);
        }

        /// <summary>
        /// Actualizar el estado de procesamiento de un documento
        /// </summary>
        [HttpPatch("{id}/processing-status")]
        public async Task<IActionResult> UpdateProcessingStatus(int id, [FromBody] UpdateProcessingStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var document = await _documentService.UpdateProcessingStatusAsync(userId, id, statusDto);

            if (document == null)
                return NotFound(new { message = "Documento no encontrado o no autorizado" });

            return Ok(document);
        }

        /// <summary>
        /// Eliminar un documento
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = GetCurrentUserId();

            // Verificar si el documento existe y pertenece al usuario
            var document = await _documentService.GetDocumentByIdAsync(userId, id);
            if (document == null)
                return NotFound(new { message = "Documento no encontrado o no autorizado" });

            var deleted = await _documentService.DeleteDocumentAsync(userId, id);
            
            if (!deleted)
                return BadRequest(new { message = "No se pudo eliminar el documento" });

            return Ok(new { message = "Documento eliminado exitosamente" });
        }
    }
}
