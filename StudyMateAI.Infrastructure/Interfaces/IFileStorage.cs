using System.IO;

namespace StudyMateAI.Infrastructure.Interfaces;

public interface IFileStorage
{
    /// <summary>
    /// Guarda un archivo en el almacenamiento y retorna una ruta o URL accesible.
    /// </summary>
    /// <param name="stream">Contenido del archivo</param>
    /// <param name="fileName">Nombre de archivo a guardar (incluye extensión)</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="relativePath">Ruta relativa donde ubicar el archivo (e.g. "{userId}/{subjectId}")</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ruta o URL resultante</returns>
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, string relativePath, CancellationToken ct);
}