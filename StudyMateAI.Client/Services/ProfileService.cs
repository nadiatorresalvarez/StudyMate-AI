using System.Net.Http.Json;
using StudyMateAI.Client.DTOs.Auth;
using StudyMateAI.Client.DTOs.Profile;

namespace StudyMateAI.Client.Services;

public class ProfileService
{
    private readonly HttpClient _http;

    public ProfileService(HttpClient http)
    {
        _http = http;
    }

    // Obtiene los datos del perfil actual
    public async Task<UserProfileDto?> GetProfileAsync()
    {
        return await _http.GetFromJsonAsync<UserProfileDto>("api/profile");
    }

    // Envía los cambios al backend
    public async Task UpdateProfileAsync(UpdateUserProfileRequest model)
    {
        var response = await _http.PutAsJsonAsync("api/profile", model);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al actualizar: {error}");
        }
    }
}