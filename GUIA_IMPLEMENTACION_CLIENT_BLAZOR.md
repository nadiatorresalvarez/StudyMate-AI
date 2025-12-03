# 🚀 Guía de Implementación: Consumo de APIs desde Blazor WASM

**Objetivo:** Proporcionar una guía práctica y lista para usar en el cliente Blazor para consumir los endpoints del API de StudyMate AI.

---

## 📋 Tabla de Contenidos

1. [Estructura de Proyecto Recomendada](#estructura-de-proyecto-recomendada)
2. [Configuración Inicial](#configuración-inicial)
3. [Servicios Reutilizables](#servicios-reutilizables)
4. [Ejemplos de Uso por Módulo](#ejemplos-de-uso-por-módulo)
5. [Componentes Blazor Prácticos](#componentes-blazor-prácticos)
6. [Manejo de Estados y Errores](#manejo-de-estados-y-errores)
7. [Testing](#testing)

---

## 📁 Estructura de Proyecto Recomendada

```
StudyMateAI.Client/
│
├── Program.cs (Configuración DI)
│
├── Services/
│   ├── _Base/
│   │   ├── ApiClientBase.cs
│   │   ├── Result.cs
│   │   └── HttpClientExtensions.cs
│   │
│   ├── Auth/
│   │   ├── IAuthService.cs
│   │   └── AuthService.cs
│   │
│   ├── Documents/
│   │   ├── IDocumentService.cs
│   │   └── DocumentService.cs
│   │
│   ├── Subjects/
│   │   ├── ISubjectService.cs
│   │   └── SubjectService.cs
│   │
│   ├── Summaries/
│   │   ├── ISummaryService.cs
│   │   └── SummaryService.cs
│   │
│   ├── Flashcards/
│   │   ├── IFlashcardService.cs
│   │   └── FlashcardService.cs
│   │
│   ├── Quiz/
│   │   ├── IQuizService.cs
│   │   └── QuizService.cs
│   │
│   └── Study/
│       ├── IStudyService.cs
│       └── StudyService.cs
│
├── Auth/
│   ├── CustomAuthStateProvider.cs
│   ├── AuthenticationMessageHandler.cs
│   └── GoogleAuthHelper.cs
│
├── DTOs/ (Espejar estructura del backend)
│   ├── Auth/
│   ├── Documents/
│   ├── Subjects/
│   ├── Summaries/
│   ├── Flashcards/
│   ├── Quiz/
│   └── Study/
│
├── Components/
│   ├── Layout/
│   ├── Common/
│   │   ├── LoadingSpinner.razor
│   │   ├── ErrorAlert.razor
│   │   └── SuccessNotification.razor
│   │
│   └── Features/
│       ├── Documents/
│       ├── Subjects/
│       ├── Summaries/
│       ├── Flashcards/
│       ├── Quiz/
│       └── Study/
│
└── wwwroot/
    ├── index.html
    ├── css/
    │   ├── app.css
    │   └── bootstrap.css
    └── js/
        └── app.js
```

---

## 🔧 Configuración Inicial

### 1. **Program.cs - Inyección de Dependencias**

```csharp
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using StudyMateAI.Client.Auth;
using StudyMateAI.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ====================================================================
// COMPONENTES RAÍZ
// ====================================================================
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ====================================================================
// CONFIGURACIÓN DE HTTPCLIENT
// ====================================================================

// API Base URL
const string apiBaseUrl = "http://localhost:5000/api";

// Cliente HTTP con interceptor de autenticación
builder.Services
    .AddScoped<AuthenticationMessageHandler>()
    .AddHttpClient("StudyMateAPI", client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .AddHttpMessageHandler<AuthenticationMessageHandler>();

// Cliente HTTP sin autenticación (para login)
builder.Services
    .AddHttpClient("PublicAPI", client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

// ====================================================================
// SERVICIOS DE AUTENTICACIÓN
// ====================================================================

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// ====================================================================
// ALMACENAMIENTO LOCAL
// ====================================================================

builder.Services.AddBlazoredLocalStorage();

// ====================================================================
// SERVICIOS DE NEGOCIO (API Clients)
// ====================================================================

// Servicios por módulo
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IStudyService, StudyService>();

// ====================================================================
// LOGGING
// ====================================================================

builder.Services.AddLogging(configure =>
{
    configure.AddBrowserConsole();
    configure.SetMinimumLevel(LogLevel.Debug);
});

// ====================================================================
// COMPONENTES ADICIONALES
// ====================================================================

builder.Services.AddScoped<NavigationManager>();

// ====================================================================
// CONSTRUCCIÓN
// ====================================================================

var host = builder.Build();
await host.RunAsync();
```

### 2. **appsettings.json (Cliente)**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "System": "Warning",
      "Microsoft": "Warning"
    }
  },
  "ApiSettings": {
    "BaseUrl": "http://localhost:5000/api",
    "Timeout": 30
  },
  "GoogleAuth": {
    "ClientId": "519517973496-6qtam58eeshie6g1ig88ublmqfb46kdh.apps.googleusercontent.com",
    "Scope": "email profile"
  }
}
```

---

## 🛠️ Servicios Reutilizables

### 1. **ApiClientBase.cs - Base para Todos los Servicios**

```csharp
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyMateAI.Client.Services._Base;

/// <summary>
/// Clase base para todos los servicios de API.
/// Proporciona métodos comunes para GET, POST, PUT, DELETE, etc.
/// </summary>
public abstract class ApiClientBase
{
    protected readonly IHttpClientFactory HttpClientFactory;
    protected readonly ILogger Logger;
    
    protected const string ApiClientName = "StudyMateAPI";
    protected const string PublicClientName = "PublicAPI";

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    protected ApiClientBase(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        HttpClientFactory = httpClientFactory;
        Logger = logger;
    }

    // ====================================================================
    // MÉTODOS HTTP
    // ====================================================================

    protected async Task<Result<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            Logger.LogDebug($"GET {endpoint}");

            var client = HttpClientFactory.CreateClient(ApiClientName);
            var response = await client.GetAsync(endpoint);

            return await HandleResponse<T>(response, endpoint);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error en GET {endpoint}");
            return Result<T>.Failure($"Error de conexión: {ex.Message}");
        }
    }

    protected async Task<Result<T>> PostAsync<T>(string endpoint, object? body = null)
    {
        try
        {
            Logger.LogDebug($"POST {endpoint}", body);

            var client = HttpClientFactory.CreateClient(ApiClientName);
            
            HttpContent? content = null;
            if (body != null)
            {
                content = JsonContent.Create(body, options: JsonOptions);
            }

            var response = await client.PostAsync(endpoint, content);
            return await HandleResponse<T>(response, endpoint);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error en POST {endpoint}");
            return Result<T>.Failure($"Error de conexión: {ex.Message}");
        }
    }

    protected async Task<Result<T>> PutAsync<T>(string endpoint, object? body = null)
    {
        try
        {
            Logger.LogDebug($"PUT {endpoint}");

            var client = HttpClientFactory.CreateClient(ApiClientName);
            var content = JsonContent.Create(body, options: JsonOptions);
            var response = await client.PutAsync(endpoint, content);

            return await HandleResponse<T>(response, endpoint);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error en PUT {endpoint}");
            return Result<T>.Failure($"Error de conexión: {ex.Message}");
        }
    }

    protected async Task<Result<T>> PatchAsync<T>(string endpoint, object? body = null)
    {
        try
        {
            Logger.LogDebug($"PATCH {endpoint}");

            var client = HttpClientFactory.CreateClient(ApiClientName);
            var content = JsonContent.Create(body, options: JsonOptions);
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
            {
                Content = content
            };

            var response = await client.SendAsync(request);
            return await HandleResponse<T>(response, endpoint);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error en PATCH {endpoint}");
            return Result<T>.Failure($"Error de conexión: {ex.Message}");
        }
    }

    protected async Task<Result<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            Logger.LogDebug($"DELETE {endpoint}");

            var client = HttpClientFactory.CreateClient(ApiClientName);
            var response = await client.DeleteAsync(endpoint);

            return await HandleResponse<T>(response, endpoint);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error en DELETE {endpoint}");
            return Result<T>.Failure($"Error de conexión: {ex.Message}");
        }
    }

    // Para endpoints públicos (sin autenticación)
    protected async Task<Result<T>> PostAsPublicAsync<T>(string endpoint, object? body = null)
    {
        try
        {
            var client = HttpClientFactory.CreateClient(PublicClientName);
            var content = JsonContent.Create(body, options: JsonOptions);
            var response = await client.PostAsync(endpoint, content);

            return await HandleResponse<T>(response, endpoint);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error en POST público {endpoint}");
            return Result<T>.Failure($"Error de conexión: {ex.Message}");
        }
    }

    // ====================================================================
    // DESCARGA DE ARCHIVOS
    // ====================================================================

    protected async Task<Result<FileDto>> DownloadFileAsync(string endpoint)
    {
        try
        {
            Logger.LogDebug($"DOWNLOAD {endpoint}");

            var client = HttpClientFactory.CreateClient(ApiClientName);
            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                return Result<FileDto>.Failure($"Error {response.StatusCode}");
            }

            var fileName = response.Content.Headers.ContentDisposition?.FileName 
                ?? "descarga";
            var contentType = response.Content.Headers.ContentType?.MediaType 
                ?? "application/octet-stream";
            var content = await response.Content.ReadAsByteArrayAsync();

            var fileDto = new FileDto
            {
                FileName = fileName.Trim('"'),
                ContentType = contentType,
                Content = content
            };

            return Result<FileDto>.Success(fileDto);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error descargando {endpoint}");
            return Result<FileDto>.Failure($"Error: {ex.Message}");
        }
    }

    // ====================================================================
    // MANEJO DE RESPUESTAS
    // ====================================================================

    protected async Task<Result<T>> HandleResponse<T>(HttpResponseMessage response, string endpoint)
    {
        var content = await response.Content.ReadAsStringAsync();

        Logger.LogDebug($"Response {response.StatusCode} from {endpoint}");

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.OK or
            System.Net.HttpStatusCode.Created =>
                await ParseResponseAsync<T>(content),

            System.Net.HttpStatusCode.NoContent =>
                Result<T>.Success(default!),

            System.Net.HttpStatusCode.BadRequest =>
                ParseValidationError<T>(content),

            System.Net.HttpStatusCode.Unauthorized =>
                Result<T>.Failure("No autorizado. Por favor inicia sesión."),

            System.Net.HttpStatusCode.Forbidden =>
                Result<T>.Failure("No tienes permiso para acceder a este recurso."),

            System.Net.HttpStatusCode.NotFound =>
                Result<T>.Failure("Recurso no encontrado."),

            System.Net.HttpStatusCode.InternalServerError =>
                Result<T>.Failure("Error interno del servidor."),

            _ => Result<T>.Failure($"Error {response.StatusCode}: {content}")
        };
    }

    private async Task<Result<T>> ParseResponseAsync<T>(string content)
    {
        try
        {
            var data = JsonSerializer.Deserialize<T>(content, JsonOptions);
            return Result<T>.Success(data!);
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Error deserializando respuesta");
            return Result<T>.Failure("Error procesando respuesta del servidor");
        }
    }

    private Result<T> ParseValidationError<T>(string content)
    {
        try
        {
            var errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(content, JsonOptions);
            var errorMessage = string.Join("; ", errors?.Values.SelectMany(v => v) ?? []);
            return Result<T>.Failure(errorMessage);
        }
        catch
        {
            return Result<T>.Failure(content);
        }
    }
}
```

### 2. **Result.cs - Patrón de Resultado Genérico**

```csharp
namespace StudyMateAI.Client.Services._Base;

/// <summary>
/// Patrón Result genérico para manejar éxito/fallo
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }

    public static Result<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data,
        ErrorMessage = null
    };

    public static Result<T> Failure(string message) => new()
    {
        IsSuccess = false,
        Data = default,
        ErrorMessage = message
    };

    public static Result<T> ValidationFailure(Dictionary<string, string[]> errors) => new()
    {
        IsSuccess = false,
        Data = default,
        ErrorMessage = "Errores de validación",
        ValidationErrors = errors
    };

    public override string ToString() => IsSuccess 
        ? $"Success: {Data}" 
        : $"Failure: {ErrorMessage}";
}
```

### 3. **AuthenticationMessageHandler.cs**

```csharp
using Blazored.LocalStorage;

namespace StudyMateAI.Client.Auth;

/// <summary>
/// HttpMessageHandler que inyecta el token JWT en cada petición
/// </summary>
public class AuthenticationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthenticationMessageHandler> _logger;

    public AuthenticationMessageHandler(
        ILocalStorageService localStorage,
        ILogger<AuthenticationMessageHandler> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync("jwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Token inyectado en request");
            }
            else
            {
                _logger.LogDebug("No hay token en localStorage");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo token de localStorage");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 4. **CustomAuthStateProvider.cs**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace StudyMateAI.Client.Auth;

/// <summary>
/// Proveedor de estado de autenticación personalizado
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync("jwtToken");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("No hay token, usuario anónimo");
                return new AuthenticationState(new ClaimsPrincipal());
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Verificar si el token está expirado
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                _logger.LogWarning("Token expirado");
                await LogoutAsync();
                return new AuthenticationState(new ClaimsPrincipal());
            }

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _logger.LogDebug($"Usuario autenticado: {user.Identity?.Name}");
            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAuthenticationStateAsync");
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }

    public async Task LoginAsync(string jwtToken)
    {
        await _localStorage.SetItemAsStringAsync("jwtToken", jwtToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        _logger.LogInformation("Usuario inició sesión");
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("jwtToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        _logger.LogInformation("Usuario cerró sesión");
    }
}
```

---

## 📡 Ejemplos de Uso por Módulo

### 1. **AuthService - Autenticación con Google**

```csharp
using StudyMateAI.Client.Auth;
using StudyMateAI.Client.DTOs.Auth;
using StudyMateAI.Client.Services._Base;

namespace StudyMateAI.Client.Services.Auth;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> GoogleLoginAsync(string googleIdToken);
    Task LogoutAsync();
}

public class AuthService : ApiClientBase, IAuthService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        ILogger<AuthService> logger,
        AuthenticationStateProvider authStateProvider)
        : base(httpClientFactory, logger)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<Result<AuthResponseDto>> GoogleLoginAsync(string googleIdToken)
    {
        var request = new { googleIdToken };
        var result = await PostAsPublicAsync<AuthResponseDto>("auth/google-login", request);

        if (result.IsSuccess && !string.IsNullOrEmpty(result.Data?.JwtToken))
        {
            // Guardar token en localStorage
            if (_authStateProvider is CustomAuthStateProvider customAuth)
            {
                await customAuth.LoginAsync(result.Data.JwtToken);
            }
        }

        return result;
    }

    public async Task LogoutAsync()
    {
        if (_authStateProvider is CustomAuthStateProvider customAuth)
        {
            await customAuth.LogoutAsync();
        }
    }
}

// DTOs
namespace StudyMateAI.Client.DTOs.Auth;

public class AuthResponseDto
{
    public string JwtToken { get; set; } = string.Empty;
    public UserProfileDto User { get; set; } = new();
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public string? EducationLevel { get; set; }
}
```

### 2. **DocumentService**

```csharp
using StudyMateAI.Client.DTOs.Documents;
using StudyMateAI.Client.Services._Base;

namespace StudyMateAI.Client.Services.Documents;

public interface IDocumentService
{
    Task<Result<List<DocumentResponseDto>>> GetAllAsync();
    Task<Result<DocumentResponseDto>> GetByIdAsync(int id);
    Task<Result<List<DocumentResponseDto>>> GetBySubjectAsync(int subjectId);
    Task<Result<List<DocumentResponseDto>>> GetByStatusAsync(string status);
    Task<Result<List<FlashcardDto>>> GetFlashcardsByDocumentAsync(int documentId);
    Task<Result<DocumentResponseDto>> UploadAsync(IBrowserFile file, int subjectId);
    Task<Result<DocumentResponseDto>> CreateAsync(CreateDocumentDto dto);
    Task<Result<DocumentResponseDto>> UpdateAsync(int id, UpdateDocumentDto dto);
    Task<Result<DocumentResponseDto>> UpdateStatusAsync(int id, string status);
    Task<Result<bool>> DeleteAsync(int id);
}

public class DocumentService : ApiClientBase, IDocumentService
{
    public DocumentService(
        IHttpClientFactory httpClientFactory,
        ILogger<DocumentService> logger)
        : base(httpClientFactory, logger)
    {
    }

    public async Task<Result<List<DocumentResponseDto>>> GetAllAsync()
    {
        return await GetAsync<List<DocumentResponseDto>>("documents");
    }

    public async Task<Result<DocumentResponseDto>> GetByIdAsync(int id)
    {
        return await GetAsync<DocumentResponseDto>($"documents/{id}");
    }

    public async Task<Result<List<DocumentResponseDto>>> GetBySubjectAsync(int subjectId)
    {
        return await GetAsync<List<DocumentResponseDto>>($"documents/subject/{subjectId}");
    }

    public async Task<Result<List<DocumentResponseDto>>> GetByStatusAsync(string status)
    {
        return await GetAsync<List<DocumentResponseDto>>($"documents/status/{status}");
    }

    public async Task<Result<List<FlashcardDto>>> GetFlashcardsByDocumentAsync(int documentId)
    {
        return await GetAsync<List<FlashcardDto>>($"documents/{documentId}/flashcards");
    }

    public async Task<Result<DocumentResponseDto>> UploadAsync(IBrowserFile file, int subjectId)
    {
        try
        {
            Logger.LogDebug($"Subiendo archivo: {file.Name}");

            var client = HttpClientFactory.CreateClient(ApiClientName);
            using var form = new MultipartFormDataContent();

            // Validar tamaño
            if (file.Size > 20_000_000)
            {
                return Result<DocumentResponseDto>.Failure("El archivo no puede exceder 20 MB");
            }

            var fileContent = new StreamContent(file.OpenReadStream());
            form.Add(fileContent, "file", file.Name);
            form.Add(new StringContent(subjectId.ToString()), "subjectId");

            var response = await client.PostAsync("documents/upload", form);
            return await HandleResponse<DocumentResponseDto>(response, "documents/upload");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error subiendo archivo");
            return Result<DocumentResponseDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<DocumentResponseDto>> CreateAsync(CreateDocumentDto dto)
    {
        return await PostAsync<DocumentResponseDto>("documents", dto);
    }

    public async Task<Result<DocumentResponseDto>> UpdateAsync(int id, UpdateDocumentDto dto)
    {
        return await PutAsync<DocumentResponseDto>($"documents/{id}", dto);
    }

    public async Task<Result<DocumentResponseDto>> UpdateStatusAsync(int id, string status)
    {
        var dto = new { processingStatus = status };
        return await PatchAsync<DocumentResponseDto>($"documents/{id}/processing-status", dto);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        return await DeleteAsync<bool>($"documents/{id}");
    }
}

// DTOs
namespace StudyMateAI.Client.DTOs.Documents;

public class DocumentResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ProcessingStatus { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public bool HasFlashcards { get; set; }
    public bool HasQuiz { get; set; }
    public bool HasSummary { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
}

public class CreateDocumentDto
{
    public string Title { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateDocumentDto
{
    public string Title { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string? ExtractedText { get; set; }
}

public class FileDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
```

### 3. **SummaryService - Descargar Resúmenes**

```csharp
using StudyMateAI.Client.DTOs.Summary;
using StudyMateAI.Client.Services._Base;

namespace StudyMateAI.Client.Services.Summaries;

public interface ISummaryService
{
    Task<Result<GenerateBriefSummaryResponseDto>> GenerateBriefAsync(int documentId);
    Task<Result<GenerateBriefSummaryResponseDto>> GenerateDetailedAsync(int documentId);
    Task<Result<GenerateBriefSummaryResponseDto>> GenerateKeyConceptsAsync(int documentId);
    Task<Result<FileDto>> DownloadAsWordAsync(int resumenId);
}

public class SummaryService : ApiClientBase, ISummaryService
{
    private readonly IJSRuntime _jsRuntime;

    public SummaryService(
        IHttpClientFactory httpClientFactory,
        ILogger<SummaryService> logger,
        IJSRuntime jsRuntime)
        : base(httpClientFactory, logger)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<Result<GenerateBriefSummaryResponseDto>> GenerateBriefAsync(int documentId)
    {
        return await PostAsync<GenerateBriefSummaryResponseDto>($"summaries/generate/{documentId}");
    }

    public async Task<Result<GenerateBriefSummaryResponseDto>> GenerateDetailedAsync(int documentId)
    {
        return await PostAsync<GenerateBriefSummaryResponseDto>($"summaries/generate-detailed/{documentId}");
    }

    public async Task<Result<GenerateBriefSummaryResponseDto>> GenerateKeyConceptsAsync(int documentId)
    {
        return await PostAsync<GenerateBriefSummaryResponseDto>($"summaries/generate-key-concepts/{documentId}");
    }

    public async Task<Result<FileDto>> DownloadAsWordAsync(int resumenId)
    {
        var result = await DownloadFileAsync($"summaries/{resumenId}/download");

        if (result.IsSuccess)
        {
            // Descargar archivo
            await _jsRuntime.InvokeVoidAsync("app.downloadFile", result.Data!.FileName, result.Data!.Content);
        }

        return result;
    }
}

// DTOs
namespace StudyMateAI.Client.DTOs.Summary;

public class GenerateBriefSummaryResponseDto
{
    public int DocumentId { get; set; }
    public int SummaryId { get; set; }
    public string SummaryText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### 4. **QuizService - Descargar Cuestionarios**

```csharp
using StudyMateAI.Client.DTOs.Quizzes;
using StudyMateAI.Client.Services._Base;

namespace StudyMateAI.Client.Services.Quiz;

public interface IQuizService
{
    Task<Result<GenerateQuizResponseDto>> GenerateAsync(int documentId, GenerateQuizRequestDto request);
    Task<Result<QuizForAttemptDto>> GetForAttemptAsync(int quizId);
    Task<Result<int>> SubmitAttemptAsync(int quizId, SubmitQuizAttemptRequestDto request);
    Task<Result<QuizAttemptResultDto>> EvaluateAttemptAsync(int attemptId);
    Task<Result<QuizAttemptResultDto>> GetAttemptResultAsync(int attemptId);
    Task<Result<QuizHistoryResponseDto>> GetHistoryAsync(int? documentId = null, int? quizId = null);
    Task<Result<FileDto>> DownloadAsPdfAsync(int quizId);
}

public class QuizService : ApiClientBase, IQuizService
{
    private readonly IJSRuntime _jsRuntime;

    public QuizService(
        IHttpClientFactory httpClientFactory,
        ILogger<QuizService> logger,
        IJSRuntime jsRuntime)
        : base(httpClientFactory, logger)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<Result<GenerateQuizResponseDto>> GenerateAsync(int documentId, GenerateQuizRequestDto request)
    {
        return await PostAsync<GenerateQuizResponseDto>($"quiz/generate/{documentId}", request);
    }

    public async Task<Result<QuizForAttemptDto>> GetForAttemptAsync(int quizId)
    {
        return await GetAsync<QuizForAttemptDto>($"quiz/{quizId}/for-attempt");
    }

    public async Task<Result<int>> SubmitAttemptAsync(int quizId, SubmitQuizAttemptRequestDto request)
    {
        return await PostAsync<int>($"quiz/{quizId}/attempts", request);
    }

    public async Task<Result<QuizAttemptResultDto>> EvaluateAttemptAsync(int attemptId)
    {
        return await PostAsync<QuizAttemptResultDto>($"quiz/attempts/{attemptId}/evaluate");
    }

    public async Task<Result<QuizAttemptResultDto>> GetAttemptResultAsync(int attemptId)
    {
        return await GetAsync<QuizAttemptResultDto>($"quiz/attempts/{attemptId}");
    }

    public async Task<Result<QuizHistoryResponseDto>> GetHistoryAsync(int? documentId = null, int? quizId = null)
    {
        var query = "";
        if (documentId.HasValue) query += $"documentId={documentId}";
        if (quizId.HasValue) query += $"{(query.Length > 0 ? "&" : "")}quizId={quizId}";

        var endpoint = $"quiz/attempts/history{(query.Length > 0 ? $"?{query}" : "")}";
        return await GetAsync<QuizHistoryResponseDto>(endpoint);
    }

    public async Task<Result<FileDto>> DownloadAsPdfAsync(int quizId)
    {
        var result = await DownloadFileAsync($"quiz/{quizId}/download");

        if (result.IsSuccess)
        {
            await _jsRuntime.InvokeVoidAsync("app.downloadFile", result.Data!.FileName, result.Data!.Content);
        }

        return result;
    }
}
```

---

## 🎨 Componentes Blazor Prácticos

### 1. **SubjectsComponent.razor - Listado de Materias**

```razor
@page "/subjects"
@using StudyMateAI.Client.Services.Subjects
@using StudyMateAI.Client.DTOs.Subject
@inject ISubjectService SubjectService
@inject NavigationManager NavManager

<div class="container mt-4">
    <div class="row mb-4">
        <div class="col-md-8">
            <h1>Mis Materias</h1>
        </div>
        <div class="col-md-4 text-end">
            <button class="btn btn-primary" @onclick="OpenCreateDialog">
                + Nueva Materia
            </button>
        </div>
    </div>

    @if (isLoading)
    {
        <div class="alert alert-info">
            <span class="spinner-border spinner-border-sm me-2"></span>
            Cargando materias...
        </div>
    }
    else if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger alert-dismissible fade show">
            @errorMessage
            <button type="button" class="btn-close" @onclick="() => errorMessage = string.Empty"></button>
        </div>
    }
    else if (subjects?.Any() != true)
    {
        <div class="alert alert-warning">
            No tienes materias creadas. ¡Comienza creando una!
        </div>
    }
    else
    {
        <div class="row g-4">
            @foreach (var subject in subjects)
            {
                <div class="col-md-4">
                    <div class="card h-100 subject-card cursor-pointer" 
                         @onclick="() => NavManager.NavigateTo($'/subject/{subject.Id}')">
                        <div class="card-body">
                            <h5 class="card-title">
                                <span style="font-size: 1.5em;">@subject.Icon</span>
                                @subject.Name
                            </h5>
                            <p class="card-text text-muted">@subject.Description</p>
                            <small class="text-secondary">
                                @subject.DocumentCount documento(s)
                            </small>
                        </div>
                        <div class="card-footer bg-light">
                            <button class="btn btn-sm btn-outline-secondary" 
                                    @onclick:stopPropagation 
                                    @onclick="() => EditSubject(subject.Id)">
                                Editar
                            </button>
                            <button class="btn btn-sm btn-outline-danger" 
                                    @onclick:stopPropagation 
                                    @onclick="() => DeleteSubject(subject.Id)">
                                Eliminar
                            </button>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
</div>

@code {
    private List<SubjectResponseDto>? subjects;
    private bool isLoading = true;
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadSubjects();
    }

    private async Task LoadSubjects()
    {
        isLoading = true;
        errorMessage = string.Empty;

        var result = await SubjectService.GetActiveAsync();

        if (result.IsSuccess)
        {
            subjects = result.Data;
        }
        else
        {
            errorMessage = result.ErrorMessage ?? "Error cargando materias";
        }

        isLoading = false;
    }

    private void OpenCreateDialog()
    {
        NavManager.NavigateTo("/subjects/new");
    }

    private void EditSubject(int id)
    {
        NavManager.NavigateTo($"/subjects/edit/{id}");
    }

    private async Task DeleteSubject(int id)
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", "¿Eliminar esta materia?"))
        {
            var result = await SubjectService.DeleteAsync(id);
            if (result.IsSuccess)
            {
                await LoadSubjects();
            }
            else
            {
                errorMessage = result.ErrorMessage ?? "Error al eliminar";
            }
        }
    }
}
```

### 2. **DocumentUploadComponent.razor**

```razor
@using StudyMateAI.Client.Services.Documents
@using StudyMateAI.Client.DTOs.Documents
@inject IDocumentService DocumentService
@inject IJSRuntime JSRuntime

<div class="card">
    <div class="card-header">
        <h5>Cargar Documento</h5>
    </div>
    <div class="card-body">
        <div class="mb-3">
            <label for="subjectSelect" class="form-label">Materia</label>
            <select class="form-control" id="subjectSelect" @bind="selectedSubjectId">
                <option value="">-- Seleccionar materia --</option>
                @foreach (var subject in Subjects ?? [])
                {
                    <option value="@subject.Id">@subject.Name</option>
                }
            </select>
        </div>

        <div class="mb-3">
            <label for="fileInput" class="form-label">Archivo (PDF, Word, Texto)</label>
            <InputFile id="fileInput" @onchange="OnFileChange" class="form-control" 
                       accept=".pdf,.doc,.docx,.txt,.odt" />
            <small class="form-text text-muted">Máximo 20 MB</small>
        </div>

        @if (!string.IsNullOrEmpty(errorMessage))
        {
            <div class="alert alert-danger">@errorMessage</div>
        }

        @if (!string.IsNullOrEmpty(successMessage))
        {
            <div class="alert alert-success">@successMessage</div>
        }

        @if (isUploading)
        {
            <div class="progress">
                <div class="progress-bar progress-bar-striped progress-bar-animated" 
                     style="width: 100%"></div>
            </div>
            <p class="text-muted">Subiendo archivo...</p>
        }
        else
        {
            <button class="btn btn-primary" @onclick="UploadFile" 
                    disabled="@(selectedFile == null || selectedSubjectId == 0)">
                Cargar Documento
            </button>
        }
    </div>
</div>

@code {
    [Parameter]
    public List<SubjectResponseDto>? Subjects { get; set; }

    [Parameter]
    public EventCallback OnUploadComplete { get; set; }

    private IBrowserFile? selectedFile;
    private int selectedSubjectId;
    private bool isUploading;
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;

    private void OnFileChange(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
        errorMessage = string.Empty;
        successMessage = string.Empty;
    }

    private async Task UploadFile()
    {
        if (selectedFile == null || selectedSubjectId == 0)
            return;

        isUploading = true;
        errorMessage = string.Empty;
        successMessage = string.Empty;

        var result = await DocumentService.UploadAsync(selectedFile, selectedSubjectId);

        if (result.IsSuccess)
        {
            successMessage = $"Documento '{selectedFile.Name}' subido correctamente";
            selectedFile = null;
            selectedSubjectId = 0;
            await OnUploadComplete.InvokeAsync();
        }
        else
        {
            errorMessage = result.ErrorMessage ?? "Error al subir el documento";
        }

        isUploading = false;
    }
}
```

### 3. **DownloadSummaryComponent.razor**

```razor
@using StudyMateAI.Client.Services.Summaries
@inject ISummaryService SummaryService
@inject IJSRuntime JSRuntime

<div class="btn-group" role="group">
    <button type="button" class="btn btn-outline-primary" 
            @onclick="() => DownloadSummary()" 
            disabled="@isDownloading">
        @if (isDownloading)
        {
            <span class="spinner-border spinner-border-sm me-2"></span>
        }
        📥 Descargar Resumen (Word)
    </button>
</div>

@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger mt-2">@errorMessage</div>
}

@code {
    [Parameter]
    public int ResumenId { get; set; }

    private bool isDownloading;
    private string errorMessage = string.Empty;

    private async Task DownloadSummary()
    {
        isDownloading = true;
        errorMessage = string.Empty;

        var result = await SummaryService.DownloadAsWordAsync(ResumenId);

        if (!result.IsSuccess)
        {
            errorMessage = result.ErrorMessage ?? "Error descargando resumen";
        }

        isDownloading = false;
    }
}
```

---

## 🎯 Manejo de Estados y Errores

### Loading State Component

```razor
@if (IsLoading)
{
    <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Cargando...</span>
    </div>
}
else if (!string.IsNullOrEmpty(ErrorMessage))
{
    <div class="alert alert-danger alert-dismissible fade show">
        <strong>Error:</strong> @ErrorMessage
        <button type="button" class="btn-close" @onclick="OnDismissError"></button>
    </div>
}
else if (HasNoData)
{
    <div class="alert alert-info">
        No hay datos para mostrar
    </div>
}
else
{
    @ChildContent
}

@code {
    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public string? ErrorMessage { get; set; }

    [Parameter]
    public bool HasNoData { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback OnDismissError { get; set; }
}
```

---

## 🧪 Testing

### Ejemplo de Test para DocumentService

```csharp
using Moq;
using Xunit;
using StudyMateAI.Client.Services.Documents;
using StudyMateAI.Client.DTOs.Documents;

namespace StudyMateAI.Client.Tests.Services;

public class DocumentServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpFactory;
    private readonly Mock<ILogger<DocumentService>> _mockLogger;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _mockHttpFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<DocumentService>>();
        _service = new DocumentService(_mockHttpFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDocuments_WhenSuccessful()
    {
        // Arrange
        var expectedDocuments = new List<DocumentResponseDto>
        {
            new() { Id = 1, Title = "Doc 1" },
            new() { Id = 2, Title = "Doc 2" }
        };

        // Act
        // var result = await _service.GetAllAsync();

        // Assert
        // Assert.True(result.IsSuccess);
        // Assert.NotNull(result.Data);
        // Assert.Equal(2, result.Data.Count);
    }
}
```

---

**Fin de la Guía de Implementación**
