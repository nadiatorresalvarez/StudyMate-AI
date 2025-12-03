using System.Net.Http.Json;
using StudyMateAI.Client.DTOs.Quiz;

namespace StudyMateAI.Client.Services;

public class QuizService
    {
        private readonly HttpClient _http;

        public QuizService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Pide al backend que genere un nuevo quiz.
        /// </summary>
        public async Task<QuizForAttemptDto> GenerateQuiz(int documentId)
        {
            var request = new { QuestionCount = 10, Difficulty = "Medium" };
    
            var response = await _http.PostAsJsonAsync($"api/Quiz/generate/{documentId}", request);
            response.EnsureSuccessStatusCode();

            // --- ESTA ES LA CORRECCIÓN ---
            // Deserializamos directamente al DTO que acabamos de crear
            var generatedQuiz = await response.Content.ReadFromJsonAsync<GenerateQuizResponseDto>();
    
            // Ahora podemos acceder a la propiedad de forma segura y fuertemente tipada
            int quizId = generatedQuiz.QuizId;

            // El resto del código sigue igual
            return await GetQuizForAttempt(quizId);
        }

        /// <summary>
        /// Obtiene la estructura de un quiz para que el usuario pueda tomarlo.
        /// </summary>
        public async Task<QuizForAttemptDto> GetQuizForAttempt(int quizId)
        {
            return await _http.GetFromJsonAsync<QuizForAttemptDto>($"api/Quiz/{quizId}/for-attempt")
                   ?? throw new Exception("Quiz no encontrado.");
        }

        /// <summary>
        /// Envía las respuestas y devuelve el resultado calificado.
        /// </summary>
        public async Task<QuizAttemptResultDto> SubmitAndEvaluate(int quizId, SubmitQuizAttemptRequestDto answers)
        {
            // PASO 1: Registrar el intento
            var submitResponse = await _http.PostAsJsonAsync($"api/Quiz/{quizId}/attempts", answers);
            submitResponse.EnsureSuccessStatusCode();
            
            // CORRECCIÓN: Usamos el DTO en lugar de dynamic
            var attemptResponse = await submitResponse.Content.ReadFromJsonAsync<SubmitAttemptResponseDto>();
            int attemptId = attemptResponse.AttemptId;

            // PASO 2: Evaluar el intento (sin cambios)
            var evaluateResponse = await _http.PostAsync($"api/Quiz/attempts/{attemptId}/evaluate", null);
            evaluateResponse.EnsureSuccessStatusCode();

            return await evaluateResponse.Content.ReadFromJsonAsync<QuizAttemptResultDto>()
                   ?? throw new Exception("No se pudo obtener el resultado del intento.");
        }

        /// <summary>
        /// (Opcional) Obtiene los resultados de un intento ya completado.
        /// </summary>
        public async Task<QuizAttemptResultDto> GetAttemptResult(int attemptId)
        {
            return await _http.GetFromJsonAsync<QuizAttemptResultDto>($"api/Quiz/attempts/{attemptId}")
                   ?? throw new Exception("Resultado no encontrado.");
        }
    }