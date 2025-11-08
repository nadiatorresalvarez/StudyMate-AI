using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace StudyMateAI.Infrastructure.Models;

public partial class dbContextStudyMateAI : DbContext
{
    public dbContextStudyMateAI()
    {
    }

    public dbContextStudyMateAI(DbContextOptions<dbContextStudyMateAI> options)
        : base(options)
    {
    }

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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=studymateai;uid=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Conceptmap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("conceptmap");

            entity.HasIndex(e => e.DocumentId, "idx_document_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.AiModelUsed)
                .HasMaxLength(100)
                .HasDefaultValueSql("'Gemini 1.5 Pro'");
            entity.Property(e => e.DocumentId).HasColumnType("int(11)");
            entity.Property(e => e.EdgeCount).HasColumnType("int(11)");
            entity.Property(e => e.EdgesJson).HasComment("Relaciones entre nodos");
            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.JsonSchemaVersion)
                .HasMaxLength(10)
                .HasDefaultValueSql("'1.0'");
            entity.Property(e => e.NodeCount).HasColumnType("int(11)");
            entity.Property(e => e.NodesJson).HasComment("Nodos del mapa conceptual");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Document).WithMany(p => p.Conceptmaps)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("conceptmap_ibfk_1");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("document");

            entity.HasIndex(e => e.ProcessingStatus, "idx_processing_status");

            entity.HasIndex(e => e.SubjectId, "idx_subject_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.FileSizeKb)
                .HasColumnType("int(11)")
                .HasColumnName("FileSizeKB");
            entity.Property(e => e.FileType)
                .HasMaxLength(20)
                .HasComment("PDF, DOCX, PPTX, TXT");
            entity.Property(e => e.FileUrl).HasMaxLength(1000);
            entity.Property(e => e.Language)
                .HasMaxLength(10)
                .HasDefaultValueSql("'es'");
            entity.Property(e => e.OriginalFileName).HasMaxLength(500);
            entity.Property(e => e.PageCount).HasColumnType("int(11)");
            entity.Property(e => e.ProcessedAt).HasColumnType("datetime");
            entity.Property(e => e.ProcessingError).HasColumnType("text");
            entity.Property(e => e.ProcessingStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Pending'")
                .HasComment("Pending, Completed, Failed");
            entity.Property(e => e.SubjectId).HasColumnType("int(11)");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Subject).WithMany(p => p.Documents)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("document_ibfk_1");
        });

        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("flashcard");

            entity.HasIndex(e => e.Difficulty, "idx_difficulty");

            entity.HasIndex(e => e.DocumentId, "idx_document_id");

            entity.HasIndex(e => e.NextReviewDate, "idx_next_review");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Answer).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Difficulty)
                .HasMaxLength(20)
                .HasComment("Easy, Medium, Hard");
            entity.Property(e => e.DocumentId).HasColumnType("int(11)");
            entity.Property(e => e.EaseFactor).HasDefaultValueSql("'2.5'");
            entity.Property(e => e.IntervalDays)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.IsManuallyEdited).HasDefaultValueSql("'0'");
            entity.Property(e => e.LastReviewedAt).HasColumnType("datetime");
            entity.Property(e => e.NextReviewDate).HasColumnType("datetime");
            entity.Property(e => e.Question).HasColumnType("text");
            entity.Property(e => e.ReviewCount)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");

            entity.HasOne(d => d.Document).WithMany(p => p.Flashcards)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("flashcard_ibfk_1");
        });

        modelBuilder.Entity<Flashcardreview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("flashcardreview");

            entity.HasIndex(e => e.FlashcardId, "idx_flashcard_id");

            entity.HasIndex(e => e.ReviewedAt, "idx_reviewed_at");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.FlashcardId).HasColumnType("int(11)");
            entity.Property(e => e.Rating)
                .HasMaxLength(20)
                .HasComment("Easy, Medium, Hard, Again");
            entity.Property(e => e.ReviewedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnType("int(11)");

            entity.HasOne(d => d.Flashcard).WithMany(p => p.Flashcardreviews)
                .HasForeignKey(d => d.FlashcardId)
                .HasConstraintName("flashcardreview_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.Flashcardreviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("flashcardreview_ibfk_2");
        });

        modelBuilder.Entity<Mindmap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("mindmap");

            entity.HasIndex(e => e.DocumentId, "idx_document_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.AiModelUsed)
                .HasMaxLength(100)
                .HasDefaultValueSql("'Gemini 1.5 Pro'");
            entity.Property(e => e.ColorScheme).HasMaxLength(50);
            entity.Property(e => e.DocumentId).HasColumnType("int(11)");
            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.JsonSchemaVersion)
                .HasMaxLength(10)
                .HasDefaultValueSql("'1.0'");
            entity.Property(e => e.NodeCount).HasColumnType("int(11)");
            entity.Property(e => e.NodesJson).HasComment("Estructura JSON del mapa mental");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Document).WithMany(p => p.Mindmaps)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("mindmap_ibfk_1");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("question");

            entity.HasIndex(e => e.OrderIndex, "idx_order");

            entity.HasIndex(e => e.QuizId, "idx_quiz_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CorrectAnswer).HasColumnType("text");
            entity.Property(e => e.Explanation).HasColumnType("text");
            entity.Property(e => e.OptionsJson)
                .HasComment("JSON con las opciones para multiple choice")
                .HasColumnType("text");
            entity.Property(e => e.OrderIndex).HasColumnType("int(11)");
            entity.Property(e => e.Points)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)");
            entity.Property(e => e.QuestionText).HasColumnType("text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .HasComment("MultipleChoice, TrueFalse, ShortAnswer");
            entity.Property(e => e.QuizId).HasColumnType("int(11)");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("question_ibfk_1");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("quiz");

            entity.HasIndex(e => e.DocumentId, "idx_document_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.DocumentId).HasColumnType("int(11)");
            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.TotalQuestions).HasColumnType("int(11)");

            entity.HasOne(d => d.Document).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("quiz_ibfk_1");
        });

        modelBuilder.Entity<Quizattempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("quizattempt");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.HasIndex(e => e.IsLatest, "idx_is_latest");

            entity.HasIndex(e => new { e.QuizId, e.UserId }, "idx_quiz_user");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.AnswersJson)
                .HasComment("JSON con todas las respuestas del usuario")
                .HasColumnType("text");
            entity.Property(e => e.AttemptNumber).HasColumnType("int(11)");
            entity.Property(e => e.CompletedAt).HasColumnType("datetime");
            entity.Property(e => e.CorrectAnswers).HasColumnType("int(11)");
            entity.Property(e => e.IsLatest).HasDefaultValueSql("'1'");
            entity.Property(e => e.QuizId).HasColumnType("int(11)");
            entity.Property(e => e.Score).HasColumnType("int(11)");
            entity.Property(e => e.StartedAt).HasColumnType("datetime");
            entity.Property(e => e.TimeSpentSeconds).HasColumnType("int(11)");
            entity.Property(e => e.TotalQuestions).HasColumnType("int(11)");
            entity.Property(e => e.UserId).HasColumnType("int(11)");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Quizattempts)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("quizattempt_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.Quizattempts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("quizattempt_ibfk_2");
        });

        modelBuilder.Entity<Studysession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("studysession");

            entity.HasIndex(e => e.DocumentId, "DocumentId");

            entity.HasIndex(e => e.ActivityType, "idx_activity_type");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(50)
                .HasComment("Flashcards, Quiz, Reading, etc.");
            entity.Property(e => e.DocumentId).HasColumnType("int(11)");
            entity.Property(e => e.DurationMinutes).HasColumnType("int(11)");
            entity.Property(e => e.EndedAt).HasColumnType("datetime");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnType("int(11)");

            entity.HasOne(d => d.Document).WithMany(p => p.Studysessions)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("studysession_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Studysessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("studysession_ibfk_1");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("subject");

            entity.HasIndex(e => e.OrderIndex, "idx_order");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasComment("Código hexadecimal: #3B82F6");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasComment("Emoji o nombre de icono");
            entity.Property(e => e.IsArchived).HasDefaultValueSql("'0'");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.OrderIndex)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.UserId).HasColumnType("int(11)");

            entity.HasOne(d => d.User).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("subject_ibfk_1");
        });

        modelBuilder.Entity<Summary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("summary");

            entity.HasIndex(e => e.DocumentId, "idx_document_id");

            entity.HasIndex(e => e.IsFavorite, "idx_favorite");

            entity.HasIndex(e => e.Type, "idx_type");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.AiModelUsed)
                .HasMaxLength(100)
                .HasDefaultValueSql("'Gemini 1.5 Pro'");
            entity.Property(e => e.DocumentId).HasColumnType("int(11)");
            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IsFavorite).HasDefaultValueSql("'0'");
            entity.Property(e => e.RegenerationCount)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasComment("Brief, Detailed, KeyConcepts");
            entity.Property(e => e.WordCount).HasColumnType("int(11)");

            entity.HasOne(d => d.Document).WithMany(p => p.Summaries)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("summary_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.GoogleId, "GoogleId").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.EducationLevel)
                .HasMaxLength(50)
                .HasComment("Universidad, Secundaria, Profesional");
            entity.Property(e => e.GoogleId).HasComment("ID único de Google OAuth");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.LastActivityAt).HasColumnType("datetime");
            entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PlanType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Free'");
            entity.Property(e => e.ProfilePicture).HasMaxLength(500);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Student'");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
