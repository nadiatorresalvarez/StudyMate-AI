using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using StudyMateAI.Client.DTOs.Document;
using StudyMateAI.Client.Services.Interfaces;

namespace StudyMateAI.Client.Services.Implementations;

/// <summary>
/// Servicio para gestionar documentos
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly HttpClient _http;

    public DocumentService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Obtiene todos los documentos del usuario, opcionalmente filtrados por materia
    /// </summary>
    public async Task<List<DocumentResponseDto>> GetAll(int? subjectId = null)
    {
        try
        {
            var url = "api/documents";
            if (subjectId.HasValue)
            {
                url = $"api/documents/subject/{subjectId}";
            }

            return await _http.GetFromJsonAsync<List<DocumentResponseDto>>(url)
                   ?? new List<DocumentResponseDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo documentos: {ex.Message}");
            return new List<DocumentResponseDto>();
        }
    }

    /// <summary>
    /// Obtiene un documento específico por ID
    /// </summary>
    public async Task<DocumentResponseDto> GetById(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<DocumentResponseDto>($"api/documents/{id}")
                   ?? throw new Exception("Documento no encontrado");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo documento {id}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Sube un archivo de documento
    /// </summary>
    public async Task UploadDocument(IBrowserFile file, int subjectId)
    {
        try
        {
            long maxFileSize = 20 * 1024 * 1024; // 20 MB

            using var content = new MultipartFormDataContent();

            // Preparar el archivo stream
            var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            // Agregar el archivo al formulario
            content.Add(fileContent, "file", file.Name);

            // Agregar el ID de la materia
            content.Add(new StringContent(subjectId.ToString()), "subjectId");

            // Enviar la solicitud
            var response = await _http.PostAsync("api/documents/upload", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al subir documento: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en UploadDocument: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Elimina un documento
    /// </summary>
    public async Task Delete(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/documents/{id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error al eliminar documento");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en Delete: {ex.Message}");
            throw;
        }
    }
}
