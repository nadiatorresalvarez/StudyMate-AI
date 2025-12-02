
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StudyMateAI.Application.Common.Abstractions;

namespace StudyMateAI.Infrastructure.Adapters.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash";
    }

    public async Task<string> GenerateBriefSummaryAsync(string documentText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");

        var prompt =
            "Resume el siguiente texto académico en 200-300 palabras. " +
            "Identifica las ideas principales y estructura el resumen en Markdown.\n\nTexto:\n" +
            documentText;

        return await CallGeminiAsync(prompt, ct);
    }

    public async Task<string> GenerateQuizJsonAsync(string documentText, int questionCount, string difficulty, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");

        var prompt =
            "Genera un cuestionario de autoevaluación a partir del siguiente texto académico. " +
            $"Crea exactamente {questionCount} preguntas de tipo MultipleChoice o TrueFalse, con dificultad {difficulty}. " +
            "Devuelve EXCLUSIVAMENTE un JSON válido con el siguiente formato: " +
            "[{" +
            "\"questionText\": \"texto de la pregunta\", " +
            "\"questionType\": \"MultipleChoice|TrueFalse\", " +
            "\"options\": [\"Opción A\", \"Opción B\", ...], " +
            "\"correctAnswer\": \"texto exacto de la opción correcta\", " +
            "\"explanation\": \"explicación corta de la respuesta\"" +
            "}]. " +
            "No añadas ningún texto antes ni después del JSON.\n\nTexto:\n" +
            documentText;

        return await CallGeminiAsync(prompt, ct);
    }

    public async Task<string> GenerateFlashcardsJsonAsync(string conceptsOrText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");

        var prompt =
            "A partir de la siguiente lista de conceptos clave o texto académico, genera entre 10 y 20 tarjetas de estudio (flashcards). " +
            "Devuelve EXCLUSIVAMENTE un JSON válido con una lista de objetos con las propiedades: question, answer y difficulty (Easy|Medium|Hard). " +
            "No añadas ningún texto antes ni después del JSON.\n\nContenido:\n" +
            conceptsOrText;

        return await CallGeminiAsync(prompt, ct);
    }

    public async Task<string> GenerateKeyConceptsAsync(string documentText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");

        var prompt =
            "Extrae entre 10 y 15 conceptos clave del siguiente texto académico. " +
            "Devuelve la respuesta en formato Markdown como una lista, donde cada elemento siga el formato '- Concepto: definición corta'. " +
            "No añadas explicaciones adicionales fuera de la lista.\n\nTexto:\n" +
            documentText;

        return await CallGeminiAsync(prompt, ct);
    }

    public async Task<string> GenerateDetailedSummaryAsync(string documentText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");

        var prompt =
            "Crea un resumen detallado de 600-800 palabras del siguiente texto académico. " +
            "Explica en profundidad los conceptos principales y organiza el contenido en secciones con subtítulos Markdown (##). " +
            "Usa negritas para resaltar términos clave y listas para enumeraciones importantes.\n\nTexto:\n" +
            documentText;

        return await CallGeminiAsync(prompt, ct);
    }

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken ct)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var url =
            $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            )
        };

        using var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Gemini API Error: {response.StatusCode}\n{error}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var candidates = json.RootElement.GetProperty("candidates");

        return candidates[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }
    
    public async Task<string> GenerateMindMapJsonAsync(string documentText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("No API Key.");

        var prompt = $@"
            Actúa como un experto en educación y visualización de datos.
            Analiza el siguiente texto y genera un Mapa Mental Jerárquico.
            
            REGLAS ESTRICTAS DE SALIDA:
            1. Devuelve SOLAMENTE un objeto JSON válido.
            2. NO uses bloques de código markdown (```json). NO escribas nada antes ni después del JSON.
            3. El idioma debe ser ESPAÑOL.
            4. Usa textos cortos y concisos para los nodos (máximo 5-7 palabras).
            
            ESTRUCTURA JSON REQUERIDA:
            {{
              ""label"": ""Idea Central"",
              ""children"": [
                {{
                  ""label"": ""Idea Secundaria"",
                  ""children"": [ ... ]
                }}
              ]
            }}

            TEXTO A ANALIZAR:
            {documentText}";

        string rawResponse = await CallGeminiAsync(prompt, ct);
        return CleanJson(rawResponse);
    }
    
    public async Task<string> GenerateConceptMapJsonAsync(string documentText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("No API Key.");

        var prompt = $@"
            Actúa como un experto en pedagogía.
            Analiza el siguiente texto y genera un Mapa Conceptual (Red de Conocimiento).
            
            REGLAS ESTRICTAS DE SALIDA:
            1. Devuelve SOLAMENTE un objeto JSON válido.
            2. NO uses bloques de código markdown (```json).
            3. Identifica los conceptos clave (Nodos) y cómo se relacionan (Aristas).
            4. Las etiquetas de las relaciones ('label') deben ser VERBOS o frases conectoras (ej: 'genera', 'es parte de', 'se divide en').
            5. Limita a un máximo de 20 conceptos clave más importantes.
            
            ESTRUCTURA JSON REQUERIDA:
            {{
              ""nodes"": [
                {{ ""id"": ""1"", ""label"": ""Concepto A"" }},
                {{ ""id"": ""2"", ""label"": ""Concepto B"" }}
              ],
              ""edges"": [
                {{ ""source"": ""1"", ""target"": ""2"", ""label"": ""causa"" }}
              ]
            }}

            TEXTO A ANALIZAR:
            {documentText}
        ";

        string rawResponse = await CallGeminiAsync(prompt, ct);
        return CleanJson(rawResponse);
    }
    
    private string CleanJson(string raw)
    {
        return raw.Replace("```json", "").Replace("```", "").Trim();
    }
}
