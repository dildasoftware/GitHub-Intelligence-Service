using GitHubIntelligenceService.Domain.Entities;

namespace GitHubIntelligenceService.Application.Interfaces;

/// <summary>
/// Harici (External) servislere erişim arayüzü.
/// GitHub API ile konuşan sınıf bu interface'i implement edecek.
/// Infrastructure katmanı bunu implemente eder.
/// </summary>
public interface IGitHubService
{
    // CancellationToken: Uzun isteklerde kullanıcı iptal ederse sunucu yorulmasın diye.
    Task<GitHubUserProfile> GetUserProfileAsync(string username, CancellationToken cancellationToken);
    
    Task<List<GitHubRepository>> GetUserRepositoriesAsync(string username, CancellationToken cancellationToken);
    
    // YENİ: Aktivite (Commit) İzleme
    Task<List<GitHubCommitActivity>> GetUserActivitiesAsync(string username, CancellationToken cancellationToken);
}

// GitHub Events/Activity Model
public class GitHubCommitActivity
{
    public DateTime CreatedAt { get; set; } // Olay Zamanı (UTC)
    public string Type { get; set; } // PushEvent, PullRequestEvent vb.
    public string RepoName { get; set; } // Repo
    public List<string> CommitMessages { get; set; } = new(); // Commit Mesajları
}

// GitHub API'sinden gelen ham profil verisi (DTO değil, Integration Model)
public class GitHubUserProfile
{
    public string Login { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Bio { get; set; }
    public int Followers { get; set; }
    public int PublicRepos { get; set; }
}
