using System.Net.Http.Json;
using StudyMateAI.Client.DTOs.Subject;
using StudyMateAI.Client.Services.Interfaces;

namespace StudyMateAI.Client.Services.Implementations;

/// <summary>
/// Servicio para gestionar materias (subjects)
/// </summary>
public class SubjectService : ISubjectService
{
    private readonly HttpClient _http;

    public SubjectService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Obtiene todas las materias del usuario
    /// </summary>
    public async Task<List<SubjectResponseDto>> GetAll()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<SubjectResponseDto>>("api/subjects")
                   ?? new List<SubjectResponseDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo materias: {ex.Message}");
            return new List<SubjectResponseDto>();
        }
    }

    /// <summary>
    /// Crea una nueva materia
    /// </summary>
    public async Task Create(CreateSubjectDto subject)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/subjects", subject);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al crear materia: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en Create: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Actualiza una materia existente
    /// </summary>
    public async Task Update(int id, UpdateSubjectDto subject)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/subjects/{id}", subject);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al actualizar materia: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en Update: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Elimina una materia
    /// </summary>
    public async Task Delete(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/subjects/{id}?force=true");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al eliminar materia: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en Delete: {ex.Message}");
            throw;
        }
    }
}
