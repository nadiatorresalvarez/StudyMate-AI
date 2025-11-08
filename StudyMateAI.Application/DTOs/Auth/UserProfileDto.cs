namespace StudyMateAI.Application.DTOs.Auth
{
    public record UserProfileDto(
        int Id,
        string Name,
        string Email,
        string? ProfilePicture,
        string? EducationLevel
    );
}