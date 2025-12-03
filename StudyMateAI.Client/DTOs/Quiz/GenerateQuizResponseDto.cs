namespace StudyMateAI.Client.DTOs.Quiz;

public class GenerateQuizResponseDto
{
    // Usa [JsonPropertyName] para asegurar que coincida con el JSON, sin importar mayúsculas/minúsculas
    [System.Text.Json.Serialization.JsonPropertyName("quizId")]
    public int QuizId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("documentId")]
    public int DocumentId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    // Agrega el resto de propiedades si las necesitas...
}