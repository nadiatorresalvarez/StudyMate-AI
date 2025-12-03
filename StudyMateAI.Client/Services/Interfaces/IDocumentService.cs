using Microsoft.AspNetCore.Components.Forms;
using StudyMateAI.Client.DTOs.Document;

namespace StudyMateAI.Client.Services.Interfaces;

/// <summary>
/// Interfaz para servicios relacionados con documentos
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Obtiene todos los documentos del usuario, opcionalmente filtrados por materia
    /// </summary>
    Task<List<DocumentResponseDto>> GetAll(int? subjectId = null);

    /// <summary>
    /// Obtiene un documento específico por ID
    /// </summary>
    Task<DocumentResponseDto> GetById(int id);

    /// <summary>
    /// Sube un archivo de documento
    /// </summary>
    Task UploadDocument(IBrowserFile file, int subjectId);

    /// <summary>
    /// Elimina un documento
    /// </summary>
    Task Delete(int id);
}
