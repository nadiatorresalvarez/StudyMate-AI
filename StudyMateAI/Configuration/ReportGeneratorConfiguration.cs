using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Infrastructure.Adapters.Reports;

namespace StudyMateAI.Configuration
{
    /// <summary>
    /// Extensión para configurar y registrar IReportGenerator en el contenedor de DI.
    /// Encapsula toda la lógica de validación de archivos, logging y configuración de reportes.
    /// </summary>
    public static class ReportGeneratorConfiguration
    {
        /// <summary>
        /// Registra IReportGenerator en el contenedor de servicios con validación de logo y logging.
        /// 
        /// Características:
        /// - Valida existencia del archivo de logo en wwwroot/images
        /// - Registra logs detallados sobre rutas y archivos encontrados
        /// - Usa ruta por defecto si el logo no se encuentra
        /// - Permite continuar sin marca de agua si el logo no existe
        /// 
        /// Ejemplo de uso en Program.cs:
        ///     builder.Services.AddReportGenerator(builder.Configuration, builder.Environment);
        /// </summary>
        /// <param name="services">Colección de servicios del contenedor DI</param>
        /// <param name="configuration">Configuración de la aplicación (appsettings.json)</param>
        /// <param name="environment">Entorno de la aplicación (Development, Production, etc.)</param>
        /// <returns>IServiceCollection para encadenamiento fluido</returns>
        public static IServiceCollection AddReportGenerator(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // Obtener factory de logging para acceso antes del build de DI
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger("StudyMateAI.Configuration.ReportGeneratorConfiguration");
            var reportLogger = loggerFactory.CreateLogger("StudyMateAI.Infrastructure.Adapters.Reports.ReportGenerator");

            // Obtener ruta del logo desde configuración o usar default
            var logoPath = configuration["ReportSettings:LogoPath"] ?? "images/logo-studymate.png";
            var fullLogoPath = ValidateAndGetLogoPath(environment, logoPath, logger);

            // Registrar ReportGenerator como Scoped con la ruta validada
            services.AddScoped<IReportGenerator>(provider => 
                new ReportGenerator(fullLogoPath, reportLogger as ILogger<ReportGenerator>));

            return services;
        }

        /// <summary>
        /// Valida la existencia del archivo de logo y retorna la ruta completa o vacía.
        /// 
        /// Proceso:
        /// 1. Construye la ruta absoluta dentro de wwwroot
        /// 2. Verifica si el archivo existe
        /// 3. Registra logs detallados
        /// 4. En caso de error, lista los archivos disponibles en el directorio
        /// 5. Retorna la ruta si existe, o string.Empty para continuar sin marca de agua
        /// </summary>
        /// <param name="environment">Entorno para acceder a WebRootPath</param>
        /// <param name="logoPath">Ruta relativa del logo desde configuración</param>
        /// <param name="logger">Logger para registrar información y errores</param>
        /// <returns>Ruta completa del logo o string.Empty si no existe</returns>
        private static string ValidateAndGetLogoPath(
            IWebHostEnvironment environment,
            string logoPath,
            ILogger logger)
        {
            try
            {
                // Construir ruta absoluta dentro de wwwroot
                var fullLogoPath = Path.Combine(environment.WebRootPath, logoPath);

                logger.LogInformation("🔍 Verificando configuración de logo para ReportGenerator:");
                logger.LogInformation("   - WebRootPath: {WebRoot}", environment.WebRootPath);
                logger.LogInformation("   - Ruta configurada: {ConfigPath}", logoPath);
                logger.LogInformation("   - Ruta completa: {FullPath}", fullLogoPath);
                logger.LogInformation("   - Directorio actual: {CurrentDir}", Directory.GetCurrentDirectory());

                // Validar que el archivo exista
                if (!File.Exists(fullLogoPath))
                {
                    logger.LogWarning("⚠️ Logo NO encontrado en: {LogoPath}", fullLogoPath);
                    logger.LogWarning("   Los documentos PDF se generarán sin marca de agua.");

                    // Diagnosticar: listar archivos disponibles
                    LogAvailableFiles(environment, logger);

                    return string.Empty; // Sin marca de agua si no existe
                }

                // Logo encontrado: registrar información
                var fileInfo = new FileInfo(fullLogoPath);
                logger.LogInformation("✅ Logo encontrado: {LogoPath} ({Size} bytes)", 
                    fullLogoPath, fileInfo.Length);

                return fullLogoPath;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error al validar ruta del logo. " +
                    "Los documentos se generarán sin marca de agua.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Registra logs de diagnóstico listando los archivos disponibles en wwwroot/images
        /// para ayudar a resolver problemas de archivo no encontrado.
        /// </summary>
        /// <param name="environment">Entorno para acceder a WebRootPath</param>
        /// <param name="logger">Logger para registrar información</param>
        private static void LogAvailableFiles(IWebHostEnvironment environment, ILogger logger)
        {
            var imagesDir = Path.Combine(environment.WebRootPath, "images");

            if (!Directory.Exists(imagesDir))
            {
                logger.LogWarning("   ❌ Directorio '{ImagesDir}' no existe en wwwroot", imagesDir);
                return;
            }

            try
            {
                var files = Directory.GetFiles(imagesDir);

                if (files.Length == 0)
                {
                    logger.LogWarning("   ⚠️ Directorio '{ImagesDir}' está vacío", imagesDir);
                    return;
                }

                logger.LogInformation("   📁 Archivos encontrados en {ImagesDir}:", imagesDir);
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var fileSize = new FileInfo(file).Length;
                    logger.LogInformation("     - {FileName} ({Size} bytes)", fileName, fileSize);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "   ❌ Error al listar archivos en '{ImagesDir}'", imagesDir);
            }
        }
    }
}
