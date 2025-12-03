namespace StudyMateAI.Client.DTOs.Auth;

/// <summary>
/// DTO para enviar el token de Google al endpoint de autenticación
/// </summary>
public class GoogleLoginDto
{
    public string IdToken { get; set; } = string.Empty;
}
