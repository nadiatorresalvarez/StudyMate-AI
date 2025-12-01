using StudyMateAI.Application.Configuration; // <-- AÑADIDO (para Application)
using StudyMateAI.Infrastructure.Configuration; // <-- AÑADIDO (para Infrastructure)
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Infrastructure.Adapters.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; // <-- AÑADIDO (para JWT)
using Microsoft.IdentityModel.Tokens; // <-- AÑADIDO (para JWT)
using Microsoft.OpenApi.Models; // <-- AÑADIDO (para Swagger)
using System.Text; // <-- AÑADIDO (para JWT)
using FluentValidation; // <-- AÑADIDO (FluentValidation)
using FluentValidation.AspNetCore; // <-- AÑADIDO (FluentValidation MVC)
using StudyMateAI.Application.Validators; // <-- AÑADIDO (CreateSubjectDtoValidator)
using StudyMateAI.Validators; // <-- AÑADIDO (UpdateUserProfileRequestValidator)

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // <-- AÑADIDO (reemplaza AddOpenApi)
builder.Services.AddApplicationServices(); // <-- ARREGLADO (así se llama tu método)
builder.Services.AddInfrastructureServices(builder.Configuration); // <-- AÑADIDO (para DB y Auth)
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// Esto permite que el Frontend (Blazor) hable con el Backend
var blazorPolicy = "AllowBlazorClient";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: blazorPolicy,
        policy =>
        {
            // AQUÍ: Más adelante, cuando creemos el proyecto Blazor,
            // tendremos que venir a verificar que este puerto coincida.
            // Por seguridad, en producción no uses AllowAnyOrigin.
            policy.WithOrigins("http://localhost:5041")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// FluentValidation: registro de validación automática y escaneo de validadores
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSubjectDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserProfileRequestValidator>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // En desarrollo
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Expiración exacta
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudyMate AI API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header (Ej: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyMate AI v1");
        // AÑADIDO: Redirigir la raíz a Swagger para probar fácil
        c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorClient"); 

app.UseAuthentication(); // <-- PRIMERO (¿Quién eres?)
app.UseAuthorization();  // <-- SEGUNDO (¿Qué puedes hacer?)
app.MapControllers();

app.Run();