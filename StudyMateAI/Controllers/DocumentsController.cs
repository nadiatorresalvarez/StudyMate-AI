using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using StudyMateAI.Application.DTOs.Document;
using StudyMateAI.Application.Features.Documents.Commands.CreateDocument;
using StudyMateAI.Application.Features.Documents.Commands.UpdateDocument;
using StudyMateAI.Application.Features.Documents.Commands.UpdateProcessingStatus;
using StudyMateAI.Application.Features.Documents.Commands.DeleteDocument;
using StudyMateAI.Application.Features.Documents.Queries.GetAllDocuments;
using StudyMateAI.Application.Features.Documents.Queries.GetDocumentById;
using StudyMateAI.Application.Features.Documents.Queries.GetDocumentsBySubject;
using StudyMateAI.Application.Features.Documents.Queries.GetDocumentsByStatus;
using StudyMateAI.Application.Features.Documents.Commands.UploadDocument;
using StudyMateAI.Application.UseCases.Flashcards.Commands;
using StudyMateAI.DTOs.Request;
using System.Security.Claims;

namespace StudyMateAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentsController(IMediator mediator)
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
        /// Obtener todos los documentos del usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            var userId = GetCurrentUserId();
            var query = new GetAllDocumentsQuery(userId);
            var documents = await _mediator.Send(query);
            return Ok(documents);
        }

        /// <summary>
        /// Obtener un documento específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            var userId = GetCurrentUserId();
            var query = new GetDocumentByIdQuery(userId, id);
            var document = await _mediator.Send(query);

            if (document == null)
                return NotFound(new { message = "Documento no encontrado" });

            return Ok(document);
        }

        /// <summary>
        /// Obtener todas las flashcards de un documento del usuario
        /// </summary>
        [HttpGet("{documentId}/flashcards")]
        public async Task<IActionResult> GetFlashcardsByDocument(int documentId)
        {
            var userId = GetCurrentUserId();
            var command = new GetFlashcardsByDocumentCommand(userId, documentId);
            try
            {
                var flashcards = await _mediator.Send(command);
                return Ok(flashcards);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener todos los documentos de una materia específica
        /// </summary>
        [HttpGet("subject/{subjectId}")]
        public async Task<IActionResult> GetDocumentsBySubject(int subjectId)
        {
            var userId = GetCurrentUserId();
            var query = new GetDocumentsBySubjectQuery(userId, subjectId);
            var documents = await _mediator.Send(query);
            return Ok(documents);
        }

        /// <summary>
        /// Obtener documentos por estado de procesamiento (Pending, Completed, Failed)
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetDocumentsByStatus(string status)
        {
            var userId = GetCurrentUserId();
            var query = new GetDocumentsByStatusQuery(userId, status);
            var documents = await _mediator.Send(query);
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
                var command = new CreateDocumentCommand(userId, createDto);
                var document = await _mediator.Send(command);
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
            var command = new UpdateDocumentCommand(userId, id, updateDto);
            var document = await _mediator.Send(command);

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
            var command = new UpdateProcessingStatusCommand(userId, id, statusDto);
            var document = await _mediator.Send(command);

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
            var getQuery = new GetDocumentByIdQuery(userId, id);
            var document = await _mediator.Send(getQuery);
            
            if (document == null)
                return NotFound(new { message = "Documento no encontrado o no autorizado" });

            var command = new DeleteDocumentCommand(userId, id);
            var deleted = await _mediator.Send(command);
            
            if (!deleted)
                return BadRequest(new { message = "No se pudo eliminar el documento" });

            return Ok(new { message = "Documento eliminado exitosamente" });
        }

        /// <summary>
        /// Subir un archivo de documento (multipart/form-data)
        /// </summary>
        [HttpPost("upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();

            await using var stream = request.File.OpenReadStream();
            var command = new UploadDocumentCommand
            {
                UserId = userId,
                SubjectId = request.SubjectId,
                Content = stream,
                OriginalFileName = request.File.FileName,
                ContentType = request.File.ContentType,
                Size = request.File.Length
            };

            try
            {
                var created = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetDocumentById), new { id = created.Id }, created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
