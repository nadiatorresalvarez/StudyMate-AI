using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using StudyMateAI.Client;
using StudyMateAI.Client.Auth;
using StudyMateAI.Client.Services;
using StudyMateAI.Client.Services.Implementations;
using StudyMateAI.Client.Services.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = "http://localhost:5071";

// Configurar HttpClient con URL base de la API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

// Servicios de UI
builder.Services.AddMudServices();

// Servicios de Almacenamiento Local
builder.Services.AddBlazoredLocalStorage();

// Servicios de Autorización
builder.Services.AddAuthorizationCore();

// Proveedor personalizado de autenticación
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Servicios de Autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

// Servicios de Dominio (con interfaces)
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();
builder.Services.AddScoped<IQuizDownloadService, QuizDownloadService>();

// Otros servicios (mantener para compatibilidad con componentes existentes)
builder.Services.AddScoped<StudyService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<QuizService>();

await builder.Build().RunAsync();