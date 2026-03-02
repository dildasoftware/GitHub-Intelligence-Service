using GitHubIntelligenceService.Application.Interfaces;
using GitHubIntelligenceService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubIntelligenceService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Business Logic Servislerinin Kaydı (Scoped: Her istek için bir tane)
        services.AddScoped<IDeveloperAnalysisService, DeveloperAnalysisService>();
        
        return services;
    }
}
