using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StudyMateAI.Application.Mappings;
using StudyMateAI.Application.UseCases.Auth;
using StudyMateAI.Application.Services;
using System.Reflection;

namespace StudyMateAI.Application.Configuration
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        
            // 1. Registra MediatR para CQRS
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            // 2. Registra AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddScoped<GoogleAuthUseCase>();

            // Registra los Servicios de Aplicación (legacy, se puede mantener o migrar a CQRS)
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IDocumentService, DocumentService>();

            return services;
        }
}
