using System;
using System.Collections.Generic;

namespace StudyMateAI.Infrastructure.Models;

public partial class User
{
    public int Id { get; set; }

    /// <summary>
    /// ID único de Google OAuth
    /// </summary>
    public string GoogleId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ProfilePicture { get; set; }

    /// <summary>
    /// Universidad, Secundaria, Profesional
    /// </summary>
    public string? EducationLevel { get; set; }

    public string? Role { get; set; }

    public string? PlanType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Flashcardreview> Flashcardreviews { get; set; } = new List<Flashcardreview>();

    public virtual ICollection<Quizattempt> Quizattempts { get; set; } = new List<Quizattempt>();

    public virtual ICollection<Studysession> Studysessions { get; set; } = new List<Studysession>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
