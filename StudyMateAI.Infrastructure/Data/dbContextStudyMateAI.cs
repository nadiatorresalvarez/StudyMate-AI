using Microsoft.EntityFrameworkCore;
using StudyMateAI.Domain.Entities; // <-- 1. AHORA SÍ USA DOMAIN.ENTITIES
using Pomelo.EntityFrameworkCore.MySql.Extensions; // <-- 2. USING CORRECTO (para MySQL)
using Microsoft.EntityFrameworkCore.Metadata.Builders; // <-- 3. USING CORRECTO (para MySQL)

// 4. Namespace correcto de Infrastructure
namespace StudyMateAI.Infrastructure.Data
{
    public partial class dbContextStudyMateAI : DbContext
    {
        // 5. Constructor ÚNICO (recibe la conexión desde Program.cs)
        public dbContextStudyMateAI(DbContextOptions<dbContextStudyMateAI> options)
            : base(options)
        {
        }

        // 6. DbSets (apuntan a Domain.Entities)
        public virtual DbSet<Conceptmap> Conceptmaps { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Flashcard> Flashcards { get; set; }
        public virtual DbSet<Flashcardreview> Flashcardreviews { get; set; }
        public virtual DbSet<Mindmap> Mindmaps { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Quiz> Quizzes { get; set; }
        public virtual DbSet<Quizattempt> Quizattempts { get; set; }
        public virtual DbSet<Studysession> Studysessions { get; set; }
        public virtual DbSet<Subject> Subjects { get; set; }
        public virtual DbSet<Summary> Summaries { get; set; }
        public virtual DbSet<User> Users { get; set; } // <-- EF busca "Users" por defecto

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 7. BORRAMOS LA CONEXIÓN HARDCODEADA Y EL #WARNING
            // ¡No debe haber NADA aquí!
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 8. MANTENEMOS TU LÓGICA DE MAPEO (¡Era valiosa!)
            // Pegamos el OnModelCreating de tu archivo original aquí
            
            modelBuilder
                .UseCollation("utf8mb4_unicode_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Conceptmap>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("ConceptMap"); // <-- 9. ARREGLADO (Coincide con tu SQL)
                // ... (Pega el resto de tu config de Conceptmap aquí)
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Document"); // <-- 9. ARREGLADO
                // ... (Pega el resto de tu config de Document aquí)
            });

            // ... (Pega todas las demás configuraciones de entidades aquí) ...

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                // 10. ¡LA CORRECCIÓN CRÍTICA!
                // Tu script SQL creó la tabla "User" (Mayúscula)
                // Tu DbSet se llama "Users" (Plural)
                // Entity Framework buscaba "Users" (Plural)
                // Esta línea le dice a EF que use la tabla "User" (Singular, Mayúscula)
                entity.ToTable("User"); 

                entity.HasIndex(e => e.Email, "Email").IsUnique();
                entity.HasIndex(e => e.GoogleId, "GoogleId").IsUnique();
                // ... (Pega el resto de tu config de User aquí)
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}