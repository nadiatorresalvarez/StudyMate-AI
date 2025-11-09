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

        public string Name { get; private set; } = null!;

        public string? ProfilePicture { get; set; }

        /// <summary>
        /// Universidad, Secundaria, Profesional
        /// </summary>
        public string? EducationLevel { get; private set; }

        public string? Role { get; set; }

        public string? PlanType { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime? LastActivityAt { get; set; }

        public bool? IsActive { get; set; }
        
        private User() { }
        
        public User(string googleId, string email, string name, string? profilePicture)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 50)
            {
                throw new ArgumentException("El nombre de usuario debe tener entre 3 y 50 caracteres.");
            }

            GoogleId = googleId;
            Email = email;
            Name = name;
            ProfilePicture = profilePicture;

            EducationLevel = "University";
            Role = "Student";
            PlanType = "Free";
            CreatedAt = DateTime.UtcNow;
            LastLoginAt = DateTime.UtcNow;
            IsActive = true;
        }
        
        public void UpdateProfile(string newName, string newEducationLevel)
        {
            // Validación (Criterio de Aceptación: El nombre debe tener entre 3 y 50 caracteres)
            if (string.IsNullOrWhiteSpace(newName) || newName.Length < 3 || newName.Length > 50)
            {
                // Lanzamos una excepción si la regla de negocio no se cumple.
                // La capa de Application se encargará de atraparla y notificar al usuario.
                throw new ArgumentException("El nombre debe tener entre 3 y 50 caracteres.");
            }

            Name = newName;
            EducationLevel = newEducationLevel;
        }

        // 2. CORRECCIÓN DE NAVEGACIÓN
        // Tus otras entidades (Flashcardreview, etc.) también deben estar en el namespace correcto
        // para que esto funcione.
        public virtual ICollection<Flashcardreview> Flashcardreviews { get; set; } = new List<Flashcardreview>();

        public virtual ICollection<Quizattempt> Quizattempts { get; set; } = new List<Quizattempt>();

        public virtual ICollection<Studysession> Studysessions { get; set; } = new List<Studysession>();

        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}