namespace StudyMateAI.Application.DTOs.Auth
{
    public record AuthResponseDto(
        string JwtToken,
        UserProfileDto User
    );
}