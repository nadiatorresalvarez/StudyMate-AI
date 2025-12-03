namespace StudyMateAI.Client.Services.Interfaces;

/// <summary>
/// Interfaz para gestionar descargas de cuestionarios
/// </summary>
public interface IQuizDownloadService
{
    /// <summary>
    /// Descarga un cuestionario en formato PDF
    /// Endpoint: GET /api/quiz/{quizId}/download
    /// </summary>
    Task<byte[]?> DownloadQuizPdfAsync(int quizId);
}
