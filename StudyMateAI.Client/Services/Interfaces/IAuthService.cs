using StudyMateAI.Client.DTOs.Auth;

namespace StudyMateAI.Client.Services.Interfaces;

/// <summary>
/// Interfaz para servicios de autenticación
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Autentica al usuario con un token de Google
    /// </summary>
    /// <param name="googleIdToken">Token obtenido desde Google Identity Services</param>
    /// <returns>True si la autenticación fue exitosa, false en caso contrario</returns>
    Task<bool> LoginWithGoogle(string googleIdToken);

    /// <summary>
    /// Cierra la sesión del usuario
    /// </summary>
    Task Logout();

    /// <summary>
    /// Obtiene el estado actual de autenticación del usuario
    /// </summary>
    /// <returns>True si el usuario está autenticado, false en caso contrario</returns>
    Task<bool> IsAuthenticated();

    /// <summary>
    /// Obtiene el token JWT almacenado
    /// </summary>
    Task<string?> GetToken();
}
