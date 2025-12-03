namespace StudyMateAI.Client.DTOs.Flashcards;

/// <summary>
/// DTO para enviar la evaluación de una flashcard al backend
/// </summary>
public class ReviewFlashcardRequestDto
{
    /// <summary>
    /// Calidad de la respuesta del usuario (0-5)
    /// 0: No lo sabía
    /// 2: Difícil
    /// 3: Bien
    /// 4: Fácil
    /// 5: Muy fácil
    /// </summary>
    public int Quality { get; set; }
}
