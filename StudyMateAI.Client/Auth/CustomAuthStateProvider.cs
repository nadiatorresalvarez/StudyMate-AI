using System.Security.Claims;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace StudyMateAI.Client.Auth;

/// <summary>
/// AuthenticationStateProvider personalizado que gestiona la autenticación con JWT
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;
    private const string AuthTokenKey = "jwtToken";
    private const string UserEmailKey = "userEmail";

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    /// <summary>
    /// Obtiene el estado de autenticación actual del usuario
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>(AuthTokenKey);

            if (string.IsNullOrEmpty(token) || JwtParser.IsTokenExpired(token))
            {
                // No hay token o está expirado -> Usuario anónimo
                ClearAuthHeaders();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Token válido -> Configurar headers y cargar claims
            SetAuthHeaders(token);
            var claims = JwtParser.ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

            return new AuthenticationState(authenticatedUser);
        }
        catch
        {
            // En caso de error, considerar usuario como anónimo
            ClearAuthHeaders();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Marca el usuario como autenticado y notifica a los componentes suscritos
    /// </summary>
    public async Task MarkUserAsAuthenticated(string token, string email = "")
    {
        try
        {
            // Guardar token y datos en localStorage
            await _localStorage.SetItemAsync(AuthTokenKey, token);
            if (!string.IsNullOrEmpty(email))
            {
                await _localStorage.SetItemAsync(UserEmailKey, email);
            }

            // Configurar headers HTTP
            SetAuthHeaders(token);

            // Crear el estado autenticado
            var claims = JwtParser.ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            // Notificar a los componentes suscritos
            NotifyAuthenticationStateChanged(authState);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al marcar usuario como autenticado: {ex.Message}");
            await MarkUserAsLoggedOut();
        }
    }

    /// <summary>
    /// Marca el usuario como desconectado
    /// </summary>
    public async Task MarkUserAsLoggedOut()
    {
        try
        {
            // Limpiar localStorage
            await _localStorage.RemoveItemAsync(AuthTokenKey);
            await _localStorage.RemoveItemAsync(UserEmailKey);

            // Limpiar headers
            ClearAuthHeaders();

            // Crear estado anónimo
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            // Notificar a los componentes suscritos
            NotifyAuthenticationStateChanged(authState);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al desconectar usuario: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el token JWT almacenado
    /// </summary>
    public async Task<string?> GetToken()
    {
        return await _localStorage.GetItemAsync<string>(AuthTokenKey);
    }

    /// <summary>
    /// Obtiene el email del usuario almacenado
    /// </summary>
    public async Task<string?> GetUserEmail()
    {
        return await _localStorage.GetItemAsync<string>(UserEmailKey);
    }

    /// <summary>
    /// Configura el header Authorization en el HttpClient
    /// </summary>
    private void SetAuthHeaders(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Limpia el header Authorization del HttpClient
    /// </summary>
    private void ClearAuthHeaders()
    {
        _http.DefaultRequestHeaders.Authorization = null;
    }
}