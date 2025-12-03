using System.Net.Http.Json;
using StudyMateAI.Client.DTOs.Flashcards;
using StudyMateAI.Client.Services.Interfaces;

namespace StudyMateAI.Client.Services.Implementations;

/// <summary>
/// Servicio para gestionar flashcards (tarjetas de estudio)
/// </summary>
public class FlashcardService : IFlashcardService
{
    private readonly HttpClient _http;

    public FlashcardService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Obtiene todas las flashcards de un documento
    /// </summary>
    public async Task<List<FlashcardResponseDto>> GetByDocumentAsync(int documentId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<FlashcardResponseDto>>($"api/Documents/{documentId}/flashcards")
                   ?? new List<FlashcardResponseDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo flashcards: {ex.Message}");
            return new List<FlashcardResponseDto>();
        }
    }

    /// <summary>
    /// Envía la evaluación de una flashcard al backend
    /// Endpoint: POST /api/flashcards/review/{flashcardId}
    /// </summary>
    public async Task ReviewFlashcardAsync(int flashcardId, int quality)
    {
        try
        {
            if (quality < 0 || quality > 5)
            {
                throw new ArgumentException("Quality debe estar entre 0 y 5");
            }

            var request = new ReviewFlashcardRequestDto { Quality = quality };
            var response = await _http.PostAsJsonAsync($"api/flashcards/review/{flashcardId}", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al revisar flashcard: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en ReviewFlashcardAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Obtiene el historial de revisiones de una flashcard
    /// </summary>
    public async Task<List<FlashcardReviewDto>> GetReviewHistoryAsync(int flashcardId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<FlashcardReviewDto>>($"api/flashcards/{flashcardId}/reviews")
                   ?? new List<FlashcardReviewDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo historial de revisiones: {ex.Message}");
            return new List<FlashcardReviewDto>();
        }
    }
}
