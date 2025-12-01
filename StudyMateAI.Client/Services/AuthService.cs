using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using StudyMateAI.Client.Auth;
using StudyMateAI.Client.DTOs.Auth;

namespace StudyMateAI.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> Login(string googleToken)
    {
        // Creamos el DTO tal como lo espera el Backend
        var request = new AuthRequestDto { GoogleIdToken = googleToken };
            
        // POST a la API
        var response = await _http.PostAsJsonAsync("api/Auth/google-login", request);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            
        // Guardamos Token y Usuario
        await _localStorage.SetItemAsync("authToken", authResponse.JwtToken);
        await _localStorage.SetItemAsync("userData", authResponse.User);

        // Actualizamos estado
        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(authResponse.JwtToken);
            
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.JwtToken);

        return true;
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("userData");
        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
        _http.DefaultRequestHeaders.Authorization = null;
    }
}