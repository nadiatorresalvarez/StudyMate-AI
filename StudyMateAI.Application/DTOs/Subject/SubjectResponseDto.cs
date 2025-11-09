using System;

namespace StudyMateAI.Application.DTOs.Subject
{
    public class SubjectResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int? OrderIndex { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsArchived { get; set; }
        public int DocumentCount { get; set; }
    }
}

