namespace StudyMateAI.Client.DTOs.Subject;

public class SubjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
        
    // CORRECCIÓN: Hacemos esto nullable para evitar errores de JSON
    public bool? IsArchived { get; set; } 
        
    public int DocumentCount { get; set; }
    
    public DateTime? CreatedAt { get; set; }
}