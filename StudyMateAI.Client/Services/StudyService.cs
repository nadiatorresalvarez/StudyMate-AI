using System.Net.Http.Json;
using StudyMateAI.Client.DTOs.Flashcards;
using StudyMateAI.Client.DTOs.Summary;
using StudyMateAI.Client.DTOs.Mindmap;
using StudyMateAI.Client.DTOs.ConceptMap;

namespace StudyMateAI.Client.Services;

public class StudyService
{
    private readonly HttpClient _http;

    public StudyService(HttpClient http)
    {
        _http = http;
    }

    // --- FLASHCARDS ---

    // 1. Obtener las ya existentes (GET)
    public async Task<List<FlashcardResponseDto>> GetFlashcards(int documentId)
    {
        // Endpoint en DocumentsController
        return await _http.GetFromJsonAsync<List<FlashcardResponseDto>>($"api/Documents/{documentId}/flashcards") 
               ?? new List<FlashcardResponseDto>();
    }

    // 2. Generar nuevas con IA (POST)
    public async Task GenerateFlashcards(int documentId)
    {
        // Endpoint en FlashcardsController
        var response = await _http.PostAsync($"api/Flashcards/generate/{documentId}", null);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error al generar flashcards.");
        }
    }
    // --- RESÚMENES ---

    public async Task<string> GenerateSummary(int documentId, string type)
    {
        // Endpoint: api/Summaries/generate/{id} (o detailed/key-concepts)
        var url = type switch
        {
            "brief" => $"api/Summaries/generate/{documentId}",
            "detailed" => $"api/Summaries/generate-detailed/{documentId}",
            "concepts" => $"api/Summaries/generate-key-concepts/{documentId}",
            _ => $"api/Summaries/generate/{documentId}"
        };

        var response = await _http.PostAsync(url, null);
            
        if (!response.IsSuccessStatusCode) 
            throw new Exception("Error generando resumen.");

        // Mapeamos la respuesta exacta del Backend
        var result = await response.Content.ReadFromJsonAsync<GenerateBriefSummaryResponseDto>();
            
        return result?.SummaryText ?? "Resumen vacío.";
    }
    // --- MAPAS ---

    public async Task<GenerateMindMapResponseDto> GenerateMindMap(int documentId)
    {
        // Creamos el objeto command que espera el Backend
        var command = new { DocumentId = documentId, UserId = 1 }; // Simulación de UserId por ahora

        // Ruta corregida: /api/Maps/mindmap/generate
        var response = await _http.PostAsJsonAsync("api/Maps/mindmap/generate", command);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GenerateMindMapResponseDto>()
               ?? throw new Exception("Respuesta inválida del servidor.");
    }

    public async Task<GenerateConceptMapResponseDto> GenerateConceptMap(int documentId)
    {
        // Creamos el objeto command que espera el Backend
        var command = new { DocumentId = documentId, UserId = 1 }; // Simulación de UserId
            
        // Ruta corregida: /api/Maps/conceptmap/generate
        var response = await _http.PostAsJsonAsync("api/Maps/conceptmap/generate", command);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GenerateConceptMapResponseDto>()
               ?? throw new Exception("Respuesta inválida del servidor.");
    }
}