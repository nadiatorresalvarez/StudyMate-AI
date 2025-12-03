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
                .AllowAnyMethod()
                .AllowCredentials();

        });

// FluentValidation: escaneo de validadores

    builder.Services.AddValidatorsFromAssemblyContaining<CreateSubjectDtoValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserProfileRequestValidator>();

// ✨ MEJORADO: ReportGenerator con validación de ruta y logging
    builder.Services.AddScoped<IReportGenerator>(provider =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var env = provider.GetRequiredService<IWebHostEnvironment>();
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();
        var reportLogger = loggerFactory.CreateLogger("StudyMateAI.Infrastructure.Adapters.Reports.ReportGenerator");

        // Obtener ruta desde configuración o usar default
        var logoPath = config["ReportSettings:LogoPath"] ?? "images/logo-studymate.png";

        // Convertir a ruta absoluta dentro de wwwroot
        var fullLogoPath = Path.Combine(env.WebRootPath, logoPath);

        logger.LogInformation("🔍 Verificando logo:");
        logger.LogInformation("   - WebRootPath: {WebRoot}", env.WebRootPath);
        logger.LogInformation("   - Ruta configurada: {ConfigPath}", logoPath);
        logger.LogInformation("   - Ruta completa: {FullPath}", fullLogoPath);
        logger.LogInformation("   - Directorio actual: {CurrentDir}", Directory.GetCurrentDirectory());

        // Validar que el archivo exista
        if (!File.Exists(fullLogoPath))
        {
            logger.LogWarning("⚠️ Logo NO encontrado en: {LogoPath}", fullLogoPath);
            logger.LogWarning("   Los documentos se generarán sin marca de agua.");

            // Listar archivos en wwwroot/images para diagnóstico
            var imagesDir = Path.Combine(env.WebRootPath, "images");
            if (Directory.Exists(imagesDir))
            {
                var files = Directory.GetFiles(imagesDir);
                logger.LogInformation("   Archivos en {ImagesDir}:", imagesDir);
                foreach (var file in files)
                {
                    logger.LogInformation("     - {FileName}", Path.GetFileName(file));
                }
            }
            else
            {
                logger.LogWarning("   ❌ Directorio 'images' no existe en wwwroot");
            }

            fullLogoPath = string.Empty; // Sin marca de agua si no existe
        }
        else
        {
            var fileInfo = new FileInfo(fullLogoPath);
            logger.LogInformation("✅ Logo encontrado: {LogoPath} ({Size} bytes)", fullLogoPath, fileInfo.Length);
        }

        return new ReportGenerator(fullLogoPath, reportLogger as ILogger<ReportGenerator>);
    });

// ✨ MEJORADO: Configuración de archivos estáticos (para el logo)
    builder.Services.AddDirectoryBrowser(); // Opcional: para debug

// ====================================================================
// AUTENTICACIÓN JWT
// ====================================================================

    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]
                                      ?? throw new InvalidOperationException(
                                          "JWT Key no configurada en appsettings.json"));

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

    app.UseCors("AllowBlazorClient");

    app.UseAuthentication(); // <-- PRIMERO (¿Quién eres?)
    app.UseAuthorization(); // <-- SEGUNDO (¿Qué puedes hacer?)

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
});
