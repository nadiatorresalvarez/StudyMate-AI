using Microsoft.EntityFrameworkCore;
using StudyMateAI.Domain.Entities;

namespace StudyMateAI.Infrastructure.Data
{
    public partial class dbContextStudyMateAI : DbContext
    {
        public dbContextStudyMateAI(DbContextOptions<dbContextStudyMateAI> options)
            : base(options)
        {
        }

        // DbSets
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
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .UseCollation("utf8mb4_unicode_ci")
                .HasCharSet("utf8mb4");

            // Configuración de Conceptmap
            modelBuilder.Entity<Conceptmap>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("ConceptMap");

                entity.HasIndex(e => e.DocumentId, "IX_conceptmaps_DocumentId");

                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.GeneratedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.Conceptmaps)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Document
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Document");

                entity.HasIndex(e => e.SubjectId, "IX_documents_SubjectId");

                entity.Property(e => e.FileName).HasMaxLength(255);
                entity.Property(e => e.OriginalFileName).HasMaxLength(255);
                entity.Property(e => e.FileType).HasMaxLength(10);
                entity.Property(e => e.FileUrl).HasMaxLength(500);
                entity.Property(e => e.Language).HasMaxLength(10);
                entity.Property(e => e.ProcessingStatus).HasMaxLength(20);
                entity.Property(e => e.UploadedAt).HasColumnType("datetime(6)");
                entity.Property(e => e.ProcessedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Subject)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Flashcard
            modelBuilder.Entity<Flashcard>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Flashcard");

                entity.HasIndex(e => e.DocumentId, "IX_flashcards_DocumentId");

                entity.Property(e => e.Difficulty).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.Flashcards)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Flashcardreview
            modelBuilder.Entity<Flashcardreview>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("FlashcardReview");

                entity.HasIndex(e => e.FlashcardId, "IX_flashcardreviews_FlashcardId");
                entity.HasIndex(e => e.UserId, "IX_flashcardreviews_UserId");

                entity.Property(e => e.Rating).HasMaxLength(20);
                entity.Property(e => e.ReviewedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Flashcard)
                    .WithMany(p => p.Flashcardreviews)
                    .HasForeignKey(d => d.FlashcardId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Flashcardreviews)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Mindmap
            modelBuilder.Entity<Mindmap>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("MindMap");

                entity.HasIndex(e => e.DocumentId, "IX_mindmaps_DocumentId");

                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.GeneratedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.Mindmaps)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Question
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Question");

                entity.HasIndex(e => e.QuizId, "IX_questions_QuizId");

                entity.Property(e => e.QuestionType).HasMaxLength(20);

                entity.HasOne(d => d.Quiz)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Quiz
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Quiz");

                entity.HasIndex(e => e.DocumentId, "IX_quizzes_DocumentId");

                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.GeneratedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.Quizzes)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Quizattempt
            modelBuilder.Entity<Quizattempt>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("QuizAttempt");

                entity.HasIndex(e => e.QuizId, "IX_quizattempts_QuizId");
                entity.HasIndex(e => e.UserId, "IX_quizattempts_UserId");

                entity.Property(e => e.StartedAt).HasColumnType("datetime(6)");
                entity.Property(e => e.CompletedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Quiz)
                    .WithMany(p => p.Quizattempts)
                    .HasForeignKey(d => d.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Quizattempts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Studysession
            modelBuilder.Entity<Studysession>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("StudySession");

                entity.HasIndex(e => e.DocumentId, "IX_studysessions_DocumentId");
                entity.HasIndex(e => e.UserId, "IX_studysessions_UserId");

                entity.Property(e => e.ActivityType).HasMaxLength(50);
                entity.Property(e => e.StartedAt).HasColumnType("datetime(6)");
                entity.Property(e => e.EndedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.Studysessions)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Studysessions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Subject
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Subject");

                entity.HasIndex(e => e.UserId, "IX_subjects_UserId");

                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).HasMaxLength(7);
                entity.Property(e => e.Icon).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Subjects)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Summary
            modelBuilder.Entity<Summary>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Summary");

                entity.HasIndex(e => e.DocumentId, "IX_summaries_DocumentId");

                entity.Property(e => e.Type).HasMaxLength(20);
                entity.Property(e => e.GeneratedAt).HasColumnType("datetime(6)");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.Summaries)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("User");

                entity.HasIndex(e => e.Email, "Email").IsUnique();
                entity.HasIndex(e => e.GoogleId, "GoogleId").IsUnique();

                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.GoogleId).HasMaxLength(255);
                entity.Property(e => e.ProfilePicture).HasMaxLength(500);
                entity.Property(e => e.EducationLevel).HasMaxLength(50);
                entity.Property(e => e.Role).HasMaxLength(20);
                entity.Property(e => e.PlanType).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime(6)");
                entity.Property(e => e.LastLoginAt).HasColumnType("datetime(6)");
                entity.Property(e => e.LastActivityAt).HasColumnType("datetime(6)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

