using Microsoft.Extensions.DependencyInjection;
using StudyMateAI.Application.Mappings;      // <-- Ahora sí existe
using StudyMateAI.Application.UseCases.Auth; // <-- Ahora sí existe
using StudyMateAI.Application.Services;

namespace StudyMateAI.Application.Configuration
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 2. Registra AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));
            
            // 3. Registra tu UseCase
            services.AddScoped<GoogleAuthUseCase>();

            // Registra los Servicios de Aplicación
            services.AddScoped<ISubjectService, SubjectService>();

            return services;
        }
    }
}