using System;
using System.Collections.Generic;

namespace StudyMateAI.Domain.Entities;

public partial class Quiz
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int TotalQuestions { get; set; }

    public int DocumentId { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Quizattempt> Quizattempts { get; set; } = new List<Quizattempt>();
}
