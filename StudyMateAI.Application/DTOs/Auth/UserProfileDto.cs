namespace StudyMateAI.Application.DTOs.Auth;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public string? EducationLevel { get; set; }
}