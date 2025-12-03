# 🔧 Guía de Código - Mejoras StudyMate AI

## Índice Rápido de Componentes

1. [FlashcardViewer.razor](#flashcardviewerrazor)
2. [FlashcardService.cs](#flashcardservicecs)
3. [SummaryService.cs](#summaryservicecs)
4. [fileDownload.js](#filedownloadjs)
5. [Cambios en DocumentDetail.razor](#cambios-en-documentdetailrazor)
6. [Cambios en QuizResult.razor](#cambios-en-quizresultrazor)

---

## FlashcardViewer.razor

**Ruta completa:** `StudyMateAI.Client/Components/FlashcardViewer.razor`

### Parámetros del Componente

```csharp
[Parameter]
public List<FlashcardResponseDto> Flashcards { get; set; } = new();

[Parameter]
public EventCallback<(int CardId, int Quality)> OnFlashcardReviewed { get; set; }
```

### Propiedades Calculadas

```csharp
private FlashcardResponseDto CurrentFlashcard =>
    CurrentCardIndex >= 0 && CurrentCardIndex < Flashcards.Count
        ? Flashcards[CurrentCardIndex]
        : new FlashcardResponseDto();

private double ProgressPercentage =>
    Flashcards.Count > 0 ? ((CurrentCardIndex + 1) / (double)Flashcards.Count) * 100 : 0;

private int ReviewedCount => ReviewedCardIds.Count;

private double ReviewedPercentage =>
    Flashcards.Count > 0 ? (ReviewedCount / (double)Flashcards.Count) * 100 : 0;
```

### Variables de Estado

```csharp
private int CurrentCardIndex = 0;              // Índice actual
private bool IsFlipped = false;                // Estado de flip
private bool IsEvaluating = false;             // Durante la evaluación
private HashSet<int> ReviewedCardIds = new(); // Tarjetas ya revisadas
```

### Métodos Principales

```csharp
/// <summary>
/// Alterna el estado de flip de la tarjeta
/// </summary>
private void ToggleFlip()
{
    IsFlipped = !IsFlipped;
}

/// <summary>
/// Navega a la siguiente tarjeta
/// </summary>
private void NextCard()
{
    if (CurrentCardIndex < Flashcards.Count - 1)
    {
        CurrentCardIndex++;
        IsFlipped = false;
    }
}

/// <summary>
/// Navega a la tarjeta anterior
/// </summary>
private void PreviousCard()
{
    if (CurrentCardIndex > 0)
    {
        CurrentCardIndex--;
        IsFlipped = false;
    }
}

/// <summary>
/// Evalúa la tarjeta actual con una calidad (0-5)
/// </summary>
private async Task EvaluateFlashcard(int quality)
{
    IsEvaluating = true;
    try
    {
        // Enviar evaluación al backend
        await FlashcardService.ReviewFlashcardAsync(CurrentFlashcard.Id, quality);
        
        // Registrar como revisada
        ReviewedCardIds.Add(CurrentFlashcard.Id);

        // Notificar al componente padre
        await OnFlashcardReviewed.InvokeAsync((CurrentFlashcard.Id, quality));

        // Feedback visual
        Snackbar.Add($"Tarjeta evaluada: {GetQualityLabel(quality)}", Severity.Success);

        // Avanzar automáticamente si no es la última
        if (CurrentCardIndex < Flashcards.Count - 1)
        {
            await Task.Delay(500);
            NextCard();
        }
    }
    catch (Exception ex)
    {
        Snackbar.Add($"Error: {ex.Message}", Severity.Error);
    }
    finally
    {
        IsEvaluating = false;
    }
}

/// <summary>
/// Convierte valor de calidad a etiqueta legible
/// </summary>
private string GetQualityLabel(int quality) => quality switch
{
    0 => "No lo sabía",
    2 => "Difícil",
    3 => "Bien",
    4 => "Fácil",
    5 => "Muy fácil",
    _ => "Desconocido"
};
```

### Estructura HTML Principal

```html
<!-- Indicador de Progreso -->
<div class="progress-indicator mb-4">
    <MudText Typo="Typo.h6" Align="Align.Center">
        Tarjeta @(CurrentCardIndex + 1) de @Flashcards.Count
    </MudText>
    <MudProgressLinear Value="@ProgressPercentage" Color="Color.Primary" />
</div>

<!-- Contenedor de la Tarjeta (Flip) -->
<div class="flashcard-wrapper mt-6 mb-6">
    <div class="flashcard @(IsFlipped ? "flipped" : "")" @onclick="ToggleFlip">
        <div class="flashcard-inner">
            <!-- Frente -->
            <div class="flashcard-front">
                <MudCard Elevation="4">
                    <MudCardContent>
                        <MudText Typo="Typo.subtitle2">Pregunta</MudText>
                        <MudText Typo="Typo.h5">@CurrentFlashcard.Question</MudText>
                    </MudCardContent>
                </MudCard>
            </div>

            <!-- Reverso -->
            <div class="flashcard-back">
                <MudCard Elevation="4">
                    <MudCardContent>
                        <MudText Typo="Typo.subtitle2">Respuesta</MudText>
                        <MudText Typo="Typo.h5">@CurrentFlashcard.Answer</MudText>
                    </MudCardContent>
                </MudCard>
            </div>
        </div>
    </div>
</div>

<!-- Botones de Navegación -->
<div class="navigation-buttons">
    <MudButton OnClick="PreviousCard" Disabled="CurrentCardIndex == 0">
        Anterior
    </MudButton>
    <MudChip>@(CurrentCardIndex + 1)/@Flashcards.Count</MudChip>
    <MudButton OnClick="NextCard" Disabled="CurrentCardIndex == Flashcards.Count - 1">
        Siguiente
    </MudButton>
</div>

<!-- Botones de Evaluación (Solo si está flipped) -->
@if (IsFlipped)
{
    <div class="evaluation-buttons">
        <MudButton OnClick="@(() => EvaluateFlashcard(0))">No lo sabía</MudButton>
        <MudButton OnClick="@(() => EvaluateFlashcard(2))">Difícil</MudButton>
        <MudButton OnClick="@(() => EvaluateFlashcard(3))">Bien</MudButton>
        <MudButton OnClick="@(() => EvaluateFlashcard(4))">Fácil</MudButton>
        <MudButton OnClick="@(() => EvaluateFlashcard(5))">Muy Fácil</MudButton>
    </div>
}
```

---

## FlashcardService.cs

**Ruta completa:** `StudyMateAI.Client/Services/Implementations/FlashcardService.cs`

```csharp
using System.Net.Http.Json;
using StudyMateAI.Client.DTOs.Flashcards;
using StudyMateAI.Client.Services.Interfaces;

namespace StudyMateAI.Client.Services.Implementations;

public class FlashcardService : IFlashcardService
{
    private readonly HttpClient _http;

    public FlashcardService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<FlashcardResponseDto>> GetByDocumentAsync(int documentId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<FlashcardResponseDto>>(
                $"api/Documents/{documentId}/flashcards"
            ) ?? new List<FlashcardResponseDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo flashcards: {ex.Message}");
            return new List<FlashcardResponseDto>();
        }
    }

    public async Task ReviewFlashcardAsync(int flashcardId, int quality)
    {
        try
        {
            if (quality < 0 || quality > 5)
            {
                throw new ArgumentException("Quality debe estar entre 0 y 5");
            }

            var request = new ReviewFlashcardRequestDto { Quality = quality };
            var response = await _http.PostAsJsonAsync(
                $"api/flashcards/review/{flashcardId}", 
                request
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al revisar: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en ReviewFlashcardAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<FlashcardReviewDto>> GetReviewHistoryAsync(int flashcardId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<FlashcardReviewDto>>(
                $"api/flashcards/{flashcardId}/reviews"
            ) ?? new List<FlashcardReviewDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo historial: {ex.Message}");
            return new List<FlashcardReviewDto>();
        }
    }
}
```

---

## SummaryService.cs

**Ruta completa:** `StudyMateAI.Client/Services/Implementations/SummaryService.cs`

```csharp
using System.Net.Http.Json;
using StudyMateAI.Client.Services.Interfaces;

namespace StudyMateAI.Client.Services.Implementations;

public class SummaryService : ISummaryService
{
    private readonly HttpClient _http;

    public SummaryService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Descarga un resumen en formato Word (.docx)
    /// Endpoint: GET /api/summaries/{resumenId}/download
    /// </summary>
    public async Task<byte[]?> DownloadSummaryAsync(int summaryId)
    {
        try
        {
            var response = await _http.GetAsync($"api/summaries/{summaryId}/download");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al descargar: {error}");
            }

            if (response.Content == null)
            {
                throw new Exception("No se recibió contenido");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en DownloadSummaryAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<SummaryDto?> GetSummaryAsync(int summaryId)
    {
        try
        {
            return await _http.GetFromJsonAsync<SummaryDto>(
                $"api/summaries/{summaryId}"
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo resumen: {ex.Message}");
            return null;
        }
    }
}
```

---

## fileDownload.js

**Ruta completa:** `StudyMateAI.Client/wwwroot/js/fileDownload.js`

```javascript
/**
 * Helpers para descargar archivos desde Blazor
 */

/**
 * Descarga un archivo desde bytes
 * @param {string} fileName - Nombre del archivo
 * @param {Uint8Array} fileContent - Contenido en bytes
 */
function downloadFileFromBytes(fileName, fileContent) {
    const blob = new Blob([fileContent], { type: 'application/octet-stream' });
    downloadFile(fileName, blob);
}

/**
 * Descarga un archivo Word (.docx)
 * @param {string} fileName - Nombre del archivo
 * @param {Uint8Array} fileContent - Contenido en bytes
 */
function downloadDocxFile(fileName, fileContent) {
    const blob = new Blob([fileContent], { 
        type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' 
    });
    downloadFile(fileName, blob);
}

/**
 * Descarga un archivo PDF
 * @param {string} fileName - Nombre del archivo
 * @param {Uint8Array} fileContent - Contenido en bytes
 */
function downloadPdfFile(fileName, fileContent) {
    const blob = new Blob([fileContent], { type: 'application/pdf' });
    downloadFile(fileName, blob);
}

/**
 * Función auxiliar para manejar la descarga
 * @private
 */
function downloadFile(fileName, blob) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName || 'descarga';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

/**
 * Abre un archivo en nueva pestaña (en lugar de descargar)
 * @param {Uint8Array} fileContent
 * @param {string} mimeType
 */
function openFileInNewTab(fileContent, mimeType) {
    const blob = new Blob([fileContent], { type: mimeType });
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
}
```

---

## Cambios en DocumentDetail.razor

### Inyecciones Necesarias

```csharp
@using StudyMateAI.Client.Services.Implementations
@inject ISummaryService SummaryService
@inject IJSRuntime JSRuntime
```

### Variables de Estado

```csharp
private int? _currentSummaryId = null;
private string _currentSummaryType = "brief";
private bool _downloadingSummary = false;
```

### Actualización de LoadSummary

```csharp
private async Task LoadSummary(string type)
{
    _loadingSummary = true;
    try
    {
        _summaryText = await StudyService.GenerateSummary(Id, type);
        _currentSummaryType = type;
        Snackbar.Add("Resumen generado", Severity.Success);
    }
    catch (Exception ex)
    {
        Snackbar.Add($"Error: {ex.Message}", Severity.Error);
    }
    finally
    {
        _loadingSummary = false;
    }
}
```

### Nuevo Método: DownloadSummary

```csharp
private async Task DownloadSummary()
{
    _downloadingSummary = true;
    try
    {
        if (_document == null || _document.SummaryCount == 0)
        {
            Snackbar.Add("No hay resumen disponible", Severity.Warning);
            return;
        }

        var summaryId = _currentSummaryId ?? 1; // Ajustar según tu backend
        var fileBytes = await SummaryService.DownloadSummaryAsync(summaryId);
        
        if (fileBytes != null && fileBytes.Length > 0)
        {
            var fileName = $"Resumen-{_document.OriginalFileName.Replace(" ", "_")}.docx";
            await JSRuntime.InvokeVoidAsync("downloadDocxFile", fileName, fileBytes);
            Snackbar.Add("Resumen descargado", Severity.Success);
        }
        else
        {
            Snackbar.Add("Error: No se pudo obtener el archivo", Severity.Error);
        }
    }
    catch (Exception ex)
    {
        Snackbar.Add($"Error: {ex.Message}", Severity.Error);
    }
    finally
    {
        _downloadingSummary = false;
    }
}
```

### Integración del Componente FlashcardViewer

```html
<FlashcardViewer 
    Flashcards="_flashcards" 
    OnFlashcardReviewed="HandleFlashcardReviewed" />
```

---

## Cambios en QuizResult.razor

### Inyecciones Necesarias

```csharp
@inject IJSRuntime JSRuntime
@inject QuizService QuizService
@inject ISnackbar Snackbar
```

### Parámetros del Componente

```csharp
[Parameter] public QuizAttemptResultDto Result { get; set; } = new();
[Parameter] public int? QuizId { get; set; }
```

### Variable de Estado

```csharp
private bool _downloadingPdf = false;
```

### Método: DownloadQuizPdf

```csharp
private async Task DownloadQuizPdf()
{
    _downloadingPdf = true;
    try
    {
        if (!QuizId.HasValue || QuizId <= 0)
        {
            Snackbar.Add("No se pudo identificar el quiz", Severity.Warning);
            return;
        }

        var fileBytes = await QuizService.DownloadQuizPdfAsync(QuizId.Value);
        
        if (fileBytes != null && fileBytes.Length > 0)
        {
            var fileName = $"Cuestionario-{Result.QuizTitle.Replace(" ", "_")}.pdf";
            await JSRuntime.InvokeVoidAsync("downloadPdfFile", fileName, fileBytes);
            Snackbar.Add("Cuestionario descargado", Severity.Success);
        }
        else
        {
            Snackbar.Add("Error al obtener el archivo", Severity.Error);
        }
    }
    catch (Exception ex)
    {
        Snackbar.Add($"Error: {ex.Message}", Severity.Error);
    }
    finally
    {
        _downloadingPdf = false;
    }
}
```

---

## Program.cs - Registro de Servicios

```csharp
// Servicios de Dominio (con interfaces)
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();

// Otros servicios
builder.Services.AddScoped<StudyService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<QuizService>();
```

---

## index.html - Carga de Scripts

```html
<!-- Antes de </head> o después de blazor.webassembly.js -->
<script src="js/fileDownload.js"></script>
```

---

## CSS Responsive para Flashcards

```css
.flashcard-container {
    padding: 20px;
    max-width: 800px;
    margin: 0 auto;
}

.flashcard-wrapper {
    perspective: 1000px;
    height: 400px;
}

.flashcard {
    position: relative;
    width: 100%;
    height: 100%;
    cursor: pointer;
    transform-style: preserve-3d;
    transition: transform 0.6s cubic-bezier(0.68, -0.55, 0.265, 1.55);
}

.flashcard.flipped {
    transform: rotateY(180deg);
}

/* Móvil */
@media (max-width: 600px) {
    .flashcard-wrapper {
        height: 300px;
    }
    
    .evaluation-btn {
        min-width: 100px;
        font-size: 0.85rem;
    }
}
```

---

## DTO: ReviewFlashcardRequestDto

```csharp
namespace StudyMateAI.Client.DTOs.Flashcards;

public class ReviewFlashcardRequestDto
{
    /// <summary>
    /// Calidad de la respuesta (0-5)
    /// 0: No lo sabía
    /// 2: Difícil
    /// 3: Bien
    /// 4: Fácil
    /// 5: Muy fácil
    /// </summary>
    public int Quality { get; set; }
}
```

---

## Ejemplo de Uso Completo

### En un Componente Padre

```csharp
@page "/estudio"
@inject IFlashcardService FlashcardService
@inject ISnackbar Snackbar

<MudContainer MaxWidth="MaxWidth.Large">
    @if (_flashcards.Count > 0)
    {
        <FlashcardViewer 
            Flashcards="_flashcards" 
            OnFlashcardReviewed="OnFlashcardReviewed" />
    }
</MudContainer>

@code {
    private List<FlashcardResponseDto> _flashcards = new();

    protected override async Task OnInitializedAsync()
    {
        _flashcards = await FlashcardService.GetByDocumentAsync(1);
    }

    private async Task OnFlashcardReviewed((int CardId, int Quality) result)
    {
        Snackbar.Add(
            $"Tarjeta {result.CardId} evaluada con calidad {result.Quality}", 
            Severity.Success
        );
    }
}
```

---

**Última actualización:** Diciembre 3, 2025
