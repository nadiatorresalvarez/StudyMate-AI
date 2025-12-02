using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using StudyMateAI.Client.DTOs.Document;

namespace StudyMateAI.Client.Services;

public class DocumentService
    {
        private readonly HttpClient _http;

        public DocumentService(HttpClient http)
        {
            _http = http;
        }

        // Obtener documentos (con filtro opcional por materia)
        public async Task<List<DocumentResponseDto>> GetAll(int? subjectId = null)
        {
            var url = "api/Documents";
            if (subjectId.HasValue)
            {
                url = $"api/Documents/subject/{subjectId}";
            }

            return await _http.GetFromJsonAsync<List<DocumentResponseDto>>(url) 
                   ?? new List<DocumentResponseDto>();
        }

        // Obtener un solo documento
        public async Task<DocumentResponseDto> GetById(int id)
        {
             return await _http.GetFromJsonAsync<DocumentResponseDto>($"api/Documents/{id}")
                    ?? throw new Exception("Documento no encontrado");
        }

        // SUBIDA DE ARCHIVOS (La parte crítica)
        public async Task UploadDocument(IBrowserFile file, int subjectId)
        {
            // Límite de tamaño (ej. 20 MB)
            long maxFileSize = 20 * 1024 * 1024; 

            using var content = new MultipartFormDataContent();
            
            // 1. Preparamos el archivo stream
            var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            // 2. Agregamos el archivo al formulario con el nombre "File" (debe coincidir con el Backend)
            content.Add(fileContent, "File", file.Name);

            // 3. Agregamos el ID de la materia
            content.Add(new StringContent(subjectId.ToString()), "SubjectId");

            // 4. Enviamos
            var response = await _http.PostAsync("api/Documents/upload", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al subir: {error}");
            }
        }

        public async Task Delete(int id)
        {
            var response = await _http.DeleteAsync($"api/Documents/{id}");
            if (!response.IsSuccessStatusCode) throw new Exception("Error al eliminar documento");
        }
    }