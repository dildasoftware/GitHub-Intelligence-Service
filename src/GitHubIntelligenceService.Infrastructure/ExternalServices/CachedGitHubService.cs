using GitHubIntelligenceService.Application.Interfaces;
using GitHubIntelligenceService.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GitHubIntelligenceService.Infrastructure.ExternalServices;

/// <summary>
/// Decorator Pattern: Orijinal GitHub servisini sarar ve araya Cache mantığını ekler.
/// Böylece "Separation of Concerns" (İlgi Ayrışımı) ilkesine uyarız.
/// GitHubService sadece API'den sorumludur, bu sınıf sadece Cache'ten sorumludur.
/// </summary>
public class CachedGitHubService : IGitHubService
{
    private readonly GitHubService _innerService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedGitHubService> _logger;

    public CachedGitHubService(GitHubService innerService, IDistributedCache cache, ILogger<CachedGitHubService> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GitHubUserProfile> GetUserProfileAsync(string username, CancellationToken cancellationToken)
    {
        string key = $"user:{username}";
        
        // 1. Cache'e bak
        string? cachedData = await _cache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Kullanıcı profili Cache'ten getirildi: {Username}", username);
            return JsonSerializer.Deserialize<GitHubUserProfile>(cachedData);
        }

        // 2. Cache'te yoksa API'ye git (Orijinal servisi çağır)
        var userProfile = await _innerService.GetUserProfileAsync(username, cancellationToken);
        
        // 3. API'den geleni Cache'e yaz (1 Saatlik Ömür)
        if (userProfile != null)
        {
            await _cache.SetStringAsync(
                key, 
                JsonSerializer.Serialize(userProfile), 
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
                cancellationToken);
        }

        return userProfile;
    }

    public async Task<List<GitHubRepository>> GetUserRepositoriesAsync(string username, CancellationToken cancellationToken)
    {
        string key = $"repos:{username}";

        string? cachedData = await _cache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Repolar Cache'ten getirildi: {Username}", username);
            return JsonSerializer.Deserialize<List<GitHubRepository>>(cachedData);
        }

        var repos = await _innerService.GetUserRepositoriesAsync(username, cancellationToken);

        if (repos != null)
        {
            await _cache.SetStringAsync(
                key, 
                JsonSerializer.Serialize(repos), 
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
                cancellationToken);
        }

        return repos;
    }

    public Task<List<GitHubCommitActivity>> GetUserActivitiesAsync(string username, CancellationToken cancellationToken)
    {
        // Aktivite verisi anlık değiştiği için cache süresini kısa tutabiliriz veya hiç cache'lemeyiz.
        // Şimdilik doğrudan servise iletiyoruz.
        return _innerService.GetUserActivitiesAsync(username, cancellationToken);
    }
}
