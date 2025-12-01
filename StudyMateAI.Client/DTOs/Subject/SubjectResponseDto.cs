namespace StudyMateAI.Client.DTOs.Subject;

public class SubjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsArchived { get; set; }
    public int DocumentCount { get; set; }
}