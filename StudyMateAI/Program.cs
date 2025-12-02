using StudyMateAI.Application.Configuration;
using StudyMateAI.Infrastructure.Configuration;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Infrastructure.Adapters.Services;
using StudyMateAI.Infrastructure.Adapters.Reports; // ✨ AÑADIDO: Para ReportGenerator
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using StudyMateAI.Application.Validators;
using StudyMateAI.Validators;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// CONFIGURACIÓN DE SERVICIOS
// ====================================================================

// Controllers y API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✨ MEJORADO: CORS (importante para aplicaciones frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // React/Vue/Angular
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Servicios de Aplicación e Infraestructura
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// HttpClient para Gemini
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// ✨ MEJORADO: FluentValidation con mejor configuración
builder.Services.AddFluentValidationAutoValidation(config =>
{
    // Deshabilitar validación implícita de DataAnnotations para evitar conflictos
    config.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateSubjectDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserProfileRequestValidator>();

// ✨ MEJORADO: ReportGenerator con validación de ruta y logging
builder.Services.AddScoped<IReportGenerator>(provider => 
{
    var config = provider.GetRequiredService<IConfiguration>();
    var env = provider.GetRequiredService<IWebHostEnvironment>();
    var logger = provider.GetRequiredService<ILogger<Program>>();
    
    // Obtener ruta desde configuración o usar default
    var logoPath = config["ReportSettings:LogoPath"] ?? "images/logo-studymate.png";
    
    // Convertir a ruta absoluta dentro de wwwroot
    var fullLogoPath = Path.Combine(env.WebRootPath, logoPath);
    
    // Validar que el archivo exista
    if (!File.Exists(fullLogoPath))
    {
        logger.LogWarning("Logo no encontrado en: {LogoPath}. Los documentos se generarán sin marca de agua.", fullLogoPath);
        // Puedes decidir si usar una ruta vacía o una imagen por defecto
        fullLogoPath = string.Empty; // Sin marca de agua si no existe
    }
    else
    {
        logger.LogInformation("Logo configurado correctamente en: {LogoPath}", fullLogoPath);
    }
    
    return new ReportGenerator(fullLogoPath);
});

// ✨ MEJORADO: Configuración de archivos estáticos (para el logo)
builder.Services.AddDirectoryBrowser(); // Opcional: para debug

// ====================================================================
// AUTENTICACIÓN JWT
// ====================================================================

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] 
    ?? throw new InvalidOperationException("JWT Key no configurada en appsettings.json"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // ⚠️ Solo en desarrollo
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
    
    // ✨ AÑADIDO: Manejo de eventos JWT para debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Error de autenticación JWT");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("Token JWT validado correctamente para usuario: {User}", 
                context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

// ====================================================================
// SWAGGER
// ====================================================================

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "StudyMate AI API", 
        Version = "v1",
        Description = "API para gestión de documentos, resúmenes y cuestionarios con IA",
        Contact = new OpenApiContact
        {
            Name = "StudyMate AI Team",
            Email = "support@studymateai.com"
        }
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando el esquema Bearer. 
                        Ingresa 'Bearer' [espacio] y luego tu token.
                        Ejemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme 
            { 
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            Array.Empty<string>()
        }
    });
    
    // ✨ AÑADIDO: Incluir comentarios XML si existen
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ====================================================================
// BUILD DE LA APLICACIÓN
// ====================================================================

var app = builder.Build();

// ====================================================================
// CONFIGURACIÓN DEL PIPELINE HTTP
// ====================================================================

// ✨ MEJORADO: Manejo de excepciones global
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyMate AI v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Colapsar por defecto
    });
}
else
{
    // ✨ AÑADIDO: Manejo de errores en producción
    app.UseExceptionHandler("/error");
    app.UseHsts(); // HTTP Strict Transport Security
}

// ✨ AÑADIDO: Servir archivos estáticos (para el logo)
app.UseStaticFiles();

app.UseHttpsRedirection();

// ✨ AÑADIDO: CORS antes de Authentication
app.UseCors("AllowFrontend");

app.UseAuthentication(); // ¿Quién eres?
app.UseAuthorization();  // ¿Qué puedes hacer?

app.MapControllers();

// ✨ AÑADIDO: Endpoint de salud (health check)
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})).AllowAnonymous();

// ✨ AÑADIDO: Endpoint de error para producción
app.MapGet("/error", () => Results.Problem("Ocurrió un error en el servidor"))
    .ExcludeFromDescription();

app.Run();