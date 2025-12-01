namespace StudyMateAI.Client.DTOs.Auth;

public class AuthResponseDto
{
    public string JwtToken { get; set; } = string.Empty;
    public UserProfileDto User { get; set; } = new();
}