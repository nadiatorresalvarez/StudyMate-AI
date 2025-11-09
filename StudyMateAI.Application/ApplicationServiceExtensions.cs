using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StudyMateAI.Application.UseCases.Auth; // <-- Ahora sí existe
using StudyMateAI.Application.Services;

namespace StudyMateAI.Application.Configuration
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registra AutoMapper. Tu forma funciona, pero esta es más robusta
            // porque encontrará cualquier perfil en el futuro, no solo 'AutoMapperProfile'.
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            
            // Registra MediatR y todos los Handlers en este proyecto.
            // Esta línea es la que conecta el IMediator del controlador
            // con nuestras clases de lógica (Query/Command Handlers).
            services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Registra los Servicios de Aplicación
            services.AddScoped<ISubjectService, SubjectService>();

            return services;
        }
    }
}