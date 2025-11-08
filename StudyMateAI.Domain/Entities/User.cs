using System;
using System.Collections.Generic;

// 1. NAMESPACE CORREGIDO
namespace StudyMateAI.Domain.Entities
{
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

        // 2. CORRECCIÓN DE NAVEGACIÓN
        // Tus otras entidades (Flashcardreview, etc.) también deben estar en el namespace correcto
        // para que esto funcione.
        public virtual ICollection<Flashcardreview> Flashcardreviews { get; set; } = new List<Flashcardreview>();

        public virtual ICollection<Quizattempt> Quizattempts { get; set; } = new List<Quizattempt>();

        public virtual ICollection<Studysession> Studysessions { get; set; } = new List<Studysession>();

        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}