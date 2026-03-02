using GitHubIntelligenceService.Application.Interfaces;
using GitHubIntelligenceService.Domain.Interfaces;
using GitHubIntelligenceService.Infrastructure.ExternalServices;
using GitHubIntelligenceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Extensions.Http;

namespace GitHubIntelligenceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Veritabanı (Persistence)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=github_intelligence.db")); // Persistent SQLite Veritabanı

        services.AddScoped<IDeveloperRepository, DeveloperRepository>();
        
        // Raporlama Servisi Kaydı
        services.AddScoped<IReportExporter, Services.HtmlReportExporter>();

        // 2. Cache (Redis veya In-Memory)
        services.AddDistributedMemoryCache(); // Şimdilik basiti seçtik.
        // services.AddStackExchangeRedisCache(o => o.Configuration = "localhost"); // Production için Redis

        // 3. Harici Servisler (External Services)
        // GitHubService'i Typed Client olarak ekleyin (Tek başına kullanılabilir)
        services.AddHttpClient<GitHubService>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "GitHubIntelligenceService"); // GitHub API kuralı
        })
        .AddPolicyHandler(GetRetryPolicy()); // Polly Entegrasyonu

        // IGitHubService istendiğinde CachedGitHubService ver (Decorator)
        services.AddScoped<IGitHubService, CachedGitHubService>();

        return services;
    }

    // Resilience için Polly Retry Politikası
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx hataları ve 408 (Timeout)
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound) // 404 durumunda retry yapmak bazen gerekebilir ama genelde yapılmaz. Burada örnek olsun diye bıraktım.
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential Backoff (2s, 4s, 8s)
    }
}
