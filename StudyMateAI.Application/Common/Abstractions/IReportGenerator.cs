using StudyMateAI.Domain.Entities;

namespace StudyMateAI.Application.Common.Abstractions;

/// <summary>
/// Puerto (Port) para la generación de reportes.
/// Define el contrato sin acoplamiento a librerías específicas (OpenXml, QuestPDF).
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// Genera un documento Word (.docx) a partir de un resumen.
    /// El procesamiento de Markdown es responsabilidad interna del adaptador.
    /// </summary>
    byte[] GenerateResumenWord(Summary resumen);

    /// <summary>
    /// Genera un documento PDF a partir de un cuestionario.
    /// </summary>
    byte[] GenerateCuestionarioPdf(Quiz cuestionario);
}

