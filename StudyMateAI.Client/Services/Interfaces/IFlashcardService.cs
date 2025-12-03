using StudyMateAI.Client.DTOs.Flashcards;

namespace StudyMateAI.Client.Services.Interfaces;

/// <summary>
/// Interfaz para gestionar flashcards (tarjetas de estudio)
/// </summary>
public interface IFlashcardService
{
    /// <summary>
    /// Obtiene todas las flashcards de un documento
    /// </summary>
    Task<List<FlashcardResponseDto>> GetByDocumentAsync(int documentId);

    /// <summary>
    /// Envía la evaluación de una flashcard al backend
    /// </summary>
    Task ReviewFlashcardAsync(int flashcardId, int quality);

    /// <summary>
    /// Obtiene el historial de revisiones de una flashcard
    /// </summary>
    Task<List<FlashcardReviewDto>> GetReviewHistoryAsync(int flashcardId);
}
