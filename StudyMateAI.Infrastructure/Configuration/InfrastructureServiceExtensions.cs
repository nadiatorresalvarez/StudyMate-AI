using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyMateAI.Infrastructure.Data;
using StudyMateAI.Domain.Interfaces;
using StudyMateAI.Infrastructure.Adapters.Repositories;
using StudyMateAI.Infrastructure.Adapters.Services;

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
            
            // Registra el Repositorio Genérico
            services.AddScoped(typeof(Domain.Interfaces.IRepository<>), typeof(Repository<>));
            
            // Registra los Repositorios específicos
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            
            //Registro del UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Registra el Servicio de Autenticación
            services.AddScoped<IAuthService, AuthService>();
            
            return services;
        }
    }
}