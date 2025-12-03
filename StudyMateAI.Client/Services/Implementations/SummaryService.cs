using System.Net.Http.Json;
using StudyMateAI.Client.Services.Interfaces;

namespace StudyMateAI.Client.Services.Implementations;

/// <summary>
/// Servicio para gestionar resúmenes incluyendo descargas
/// </summary>
public class SummaryService : ISummaryService
{
    private readonly HttpClient _http;

    public SummaryService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Descarga un resumen en formato Word (.docx)
    /// Endpoint: GET /api/summaries/{resumenId}/download
    /// </summary>
    public async Task<byte[]?> DownloadSummaryAsync(int summaryId)
    {
        try
        {
            var response = await _http.GetAsync($"api/summaries/{summaryId}/download");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al descargar resumen: {error}");
            }

            // Verificar que la respuesta tenga contenido
            if (response.Content == null)
            {
                throw new Exception("No se recibió contenido del archivo");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en DownloadSummaryAsync: {ex.Message}");
            throw;
        }
    }

}
