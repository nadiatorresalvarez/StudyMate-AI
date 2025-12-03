using System.Text.Json.Serialization;

namespace StudyMateAI.Client.DTOs.Quiz;

public class SubmitAttemptResponseDto
{
    [JsonPropertyName("attemptId")]
    public int AttemptId { get; set; }
}