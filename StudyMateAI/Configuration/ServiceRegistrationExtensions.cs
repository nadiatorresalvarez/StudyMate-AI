using Microsoft.OpenApi.Models;
using StudyMateAI.Infrastructure.Configuration;

namespace StudyMateAI.Configuration;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register application services here
        services.AddHttpContextAccessor();
        
        // Pass configuration to AddInfrastructureServices
        services.AddInfrastructureServices(configuration);
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StudyMateIA",
                Version = "v1",
                Description = "API para gestionar StudyMateIA"
            });
        });
        
        return services;
    }
}