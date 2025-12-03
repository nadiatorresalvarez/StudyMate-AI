using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using StudyMateAI.Client.Auth;
using StudyMateAI.Client.DTOs.Auth;
using StudyMateAI.Client.Services.Interfaces;

namespace StudyMateAI.Client.Services.Implementations;

/// <summary>
/// Servicio de autenticación que gestiona login, logout y estado de sesión
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Autentica al usuario con un token de Google
    /// </summary>
    public async Task<bool> LoginWithGoogle(string googleIdToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(googleIdToken))
            {
                return false;
            }

            // Crear request con el token de Google
            var request = new AuthRequestDto
            {
                GoogleIdToken = googleIdToken
            };

            // Enviar a la API
            var response = await _http.PostAsJsonAsync("api/auth/google-login", request);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            // Leer respuesta
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            if (authResponse == null || string.IsNullOrEmpty(authResponse.JwtToken))
            {
                return false;
            }

            // Marcar usuario como autenticado
            var customAuthStateProvider = (CustomAuthStateProvider)_authStateProvider;
            var email = authResponse.User?.Email ?? string.Empty;
            await customAuthStateProvider.MarkUserAsAuthenticated(authResponse.JwtToken, email);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en LoginWithGoogle: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario
    /// </summary>
    public async Task Logout()
    {
        try
        {
            var customAuthStateProvider = (CustomAuthStateProvider)_authStateProvider;
            await customAuthStateProvider.MarkUserAsLoggedOut();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en Logout: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el estado actual de autenticación del usuario
    /// </summary>
    public async Task<bool> IsAuthenticated()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Obtiene el token JWT almacenado
    /// </summary>
    public async Task<string?> GetToken()
    {
        var customAuthStateProvider = (CustomAuthStateProvider)_authStateProvider;
        return await customAuthStateProvider.GetToken();
    }
}
