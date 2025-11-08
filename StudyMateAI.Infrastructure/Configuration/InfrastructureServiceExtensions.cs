using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyMateAI.Infrastructure.Data; // <-- ARREGLADO (usando .Data)
using StudyMateAI.Domain.Interfaces; // <-- AÑADIDO (para las interfaces)
using StudyMateAI.Infrastructure.Adapters.Repositories; // <-- AÑADIDO (para las clases)
using StudyMateAI.Infrastructure.Adapters.Services; // <-- AÑADIDO (para las clases)

namespace StudyMateAI.Infrastructure.Configuration
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuracion de la conexión a la base de datos
            services.AddDbContext<dbContextStudyMateAI>((serviceProvider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
            
            // Registra el Repositorio
            services.AddScoped<IUserRepository, UserRepository>();
            
            //Registro del UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Registra el Servicio de Autenticación
            services.AddScoped<IAuthService, AuthService>();
            
            // ==========================================================
            
            return services;
        }
    }
}