using System.Threading;
using System.Threading.Tasks;

namespace StudyMateAI.Application.Common.Abstractions;

public interface IGeminiService
{
    Task<string> GenerateBriefSummaryAsync(string documentText, CancellationToken ct);
    Task<string> GenerateDetailedSummaryAsync(string documentText, CancellationToken ct);
    Task<string> GenerateKeyConceptsAsync(string documentText, CancellationToken ct);
    Task<string> GenerateFlashcardsJsonAsync(string conceptsOrText, CancellationToken ct);
    Task<string> GenerateQuizJsonAsync(string documentText, int questionCount, string difficulty, CancellationToken ct);
}
