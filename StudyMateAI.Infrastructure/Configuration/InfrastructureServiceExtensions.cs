using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyMateAI.Infrastructure.Data;
using StudyMateAI.Domain.Interfaces;
using StudyMateAI.Infrastructure.Adapters.Repositories;
using StudyMateAI.Infrastructure.Adapters.Reports;
using StudyMateAI.Infrastructure.Adapters.Services;
using StudyMateAI.Application.Common.Abstractions;
using StudyMateAI.Infrastructure.Adapters.Storage;
using Pomelo.EntityFrameworkCore.MySql;

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
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
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

            // Almacenamiento de archivos (local por ahora)
            services.AddScoped<IFileStorage, LocalFileStorage>();
            
            // Registra el generador de reportes
            services.AddScoped<IReportGenerator, ReportGenerator>();
            
            return services;
        }
    }
}