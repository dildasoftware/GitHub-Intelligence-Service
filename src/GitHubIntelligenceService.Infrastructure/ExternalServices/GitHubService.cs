using GitHubIntelligenceService.Application.Interfaces;
using GitHubIntelligenceService.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace GitHubIntelligenceService.Infrastructure.ExternalServices;

/// <summary>
/// GitHub API ile iletişim kuran "Clean Implementation".
/// Polly (Retry) ve Caching stratejilerini bu katman yönetir.
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    // Cache yönetimi (sonra eklenebilir)

    public GitHubService(HttpClient httpClient) // IHttpClientFactory tarafından oluşturulan client gelecek.
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        
        // GitHub API User-Agent zorunluluğu:
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GitHubIntelligenceService");
        // Token varsa burada eklenmeli (Authorization):
        // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "TOKEN");
    }

    public async Task<GitHubUserProfile> GetUserProfileAsync(string username, CancellationToken cancellationToken)
    {
        try 
        {
            var response = await _httpClient.GetAsync($"users/{username}", cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) 
            {
                // Kullanıcı bulunamadı.
                return null;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new Exception("GitHub API Rate Limit (İstek Limiti) aşıldı. Lütfen daha sonra deneyin.");
            }
            
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<GitHubUserProfile>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            // Loglama yapılabilir.
            throw new Exception($"GitHub API Hatası: {ex.Message}");
        }
    }

    public async Task<List<GitHubRepository>> GetUserRepositoriesAsync(string username, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"users/{username}/repos?per_page=100", cancellationToken);
        response.EnsureSuccessStatusCode();

        // API'den gelen veriyi Domain Objesi'ne dönüştür.
        // Ham GitHub repo modelini burada tanımlamış olabiliriz veya Application'da.
        // Şimdilik Domain entity'yi DTO gibi kullanıyoruz.
        // Gerçek dünyada burada bir AutoMapper veya mapping logic olmalı.
        var repos = await response.Content.ReadFromJsonAsync<List<GitHubRepository>>(cancellationToken: cancellationToken);
        return repos ?? new List<GitHubRepository>();
    }

    public async Task<List<GitHubCommitActivity>> GetUserActivitiesAsync(string username, CancellationToken cancellationToken)
    {
        // GitHub API: Kullanıcının son 30-90 günlük açık aktivitelerini getirir.
        var response = await _httpClient.GetAsync($"users/{username}/events/public?per_page=30", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            // Aktivite bulunamazsa veya hata olursa boş liste dönelim, sistemi kitlemeyelim (Fail Gracefully)
            return new List<GitHubCommitActivity>();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var activities = new List<GitHubCommitActivity>();

        try
        {
            using (JsonDocument doc = JsonDocument.Parse(content))
            {
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in root.EnumerateArray())
                    {
                        // Sadece Push (Kod Atma) Olaylarını Alalım
                        if (element.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "PushEvent")
                        {
                            var activity = new GitHubCommitActivity
                            {
                                CreatedAt = element.GetProperty("created_at").GetDateTime(),
                                Type = "PushEvent",
                                RepoName = element.GetProperty("repo").GetProperty("name").GetString() ?? "Unknown"
                            };

                            // Payload içindeki commit mesajlarını al
                            if (element.TryGetProperty("payload", out var payloadEl) && payloadEl.TryGetProperty("commits", out var commitsEl))
                            {
                                foreach (var commit in commitsEl.EnumerateArray())
                                {
                                    if (commit.TryGetProperty("message", out var msgEl))
                                    {
                                        activity.CommitMessages.Add(msgEl.GetString() ?? "");
                                    }
                                }
                            }

                            activities.Add(activity);
                            // Veri çok şişmesin, son 20 aktivite yeter.
                            if (activities.Count >= 20) break;
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // JSON hatası olursa yutalım, aktivite verisi kritik değil (Optional Feature)
        }

        return activities;
    }
}
