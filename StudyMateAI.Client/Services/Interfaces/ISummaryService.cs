namespace StudyMateAI.Client.Services.Interfaces;

/// <summary>
/// Interfaz para gestionar resúmenes incluyendo descargas
/// </summary>
public interface ISummaryService
{
    /// <summary>
    /// Descarga un resumen en formato Word (.docx)
    /// Endpoint: GET /api/summaries/{resumenId}/download
    /// </summary>
    Task<byte[]?> DownloadSummaryAsync(int summaryId);
}
