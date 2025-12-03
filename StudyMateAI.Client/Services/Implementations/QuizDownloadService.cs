using System.Net.Http.Json;
using StudyMateAI.Client.Services.Interfaces;

namespace StudyMateAI.Client.Services.Implementations;

/// <summary>
/// Servicio para descargar cuestionarios en PDF
/// </summary>
public class QuizDownloadService : IQuizDownloadService
{
    private readonly HttpClient _http;

    public QuizDownloadService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Descarga un cuestionario en formato PDF
    /// Endpoint: GET /api/quiz/{quizId}/download
    /// </summary>
    public async Task<byte[]?> DownloadQuizPdfAsync(int quizId)
    {
        try
        {
            var response = await _http.GetAsync($"api/quiz/{quizId}/download");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al descargar cuestionario: {error}");
            }

            if (response.Content == null)
            {
                throw new Exception("No se recibió contenido del archivo PDF");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en DownloadQuizPdfAsync: {ex.Message}");
            throw;
        }
    }
}
