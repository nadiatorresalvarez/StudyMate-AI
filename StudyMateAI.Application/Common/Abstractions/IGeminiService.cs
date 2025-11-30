using System.Threading;
using System.Threading.Tasks;

namespace StudyMateAI.Application.Common.Abstractions;

public interface IGeminiService
{
    Task<string> GenerateBriefSummaryAsync(string documentText, CancellationToken ct);
}
