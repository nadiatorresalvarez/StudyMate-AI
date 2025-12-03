using Blazor.Diagrams.Core;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using StudyMateAI.Client;
using StudyMateAI.Client.Auth;
using StudyMateAI.Client.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = "http://localhost:5071";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

// Servicios de UI
builder.Services.AddMudServices();

// Servicios de Auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<StudyMateAI.Client.Services.SubjectService>();
builder.Services.AddScoped<StudyMateAI.Client.Services.DocumentService>();
builder.Services.AddScoped<StudyService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<QuizService>();

builder.Services.AddScoped<Diagram>();

await builder.Build().RunAsync();