using System.Net.Http.Json;
using StudyMateAI.Client.DTOs.Subject;

namespace StudyMateAI.Client.Services;

public class SubjectService
{
    private readonly HttpClient _http;

    public SubjectService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<SubjectResponseDto>> GetAll()
    {
        // GetFromJsonAsync lanza excepción si el JSON está mal formado, 
        // lo cual es lo que queremos para ver el error en consola.
        return await _http.GetFromJsonAsync<List<SubjectResponseDto>>("api/Subjects") 
               ?? new List<SubjectResponseDto>();
    }

    public async Task Create(CreateSubjectDto subject)
    {
        var response = await _http.PostAsJsonAsync("api/Subjects", subject);
        if (!response.IsSuccessStatusCode)
        {
            // Esto leerá el mensaje de error del backend (ej. validaciones)
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al crear: {error}");
        }
    }

    public async Task Update(int id, UpdateSubjectDto subject)
    {
        var response = await _http.PutAsJsonAsync($"api/Subjects/{id}", subject);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al actualizar: {error}");
        }
    }

    public async Task Delete(int id)
    {
        var response = await _http.DeleteAsync($"api/Subjects/{id}?force=true");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al eliminar: {error}");
        }
    }
}