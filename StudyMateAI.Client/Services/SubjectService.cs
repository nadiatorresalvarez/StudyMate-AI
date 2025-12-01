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
        return await _http.GetFromJsonAsync<List<SubjectResponseDto>>("api/Subjects") 
               ?? new List<SubjectResponseDto>();
    }

    public async Task Create(CreateSubjectDto subject)
    {
        await _http.PostAsJsonAsync("api/Subjects", subject);
    }

    public async Task Update(int id, UpdateSubjectDto subject)
    {
        await _http.PutAsJsonAsync($"api/Subjects/{id}", subject);
    }

    public async Task Delete(int id)
    {
        // force=true permite borrar materias aunque tengan documentos (opcional)
        await _http.DeleteAsync($"api/Subjects/{id}?force=true");
    }
}