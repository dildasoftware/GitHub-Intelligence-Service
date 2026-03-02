using GitHubIntelligenceService.Domain.Common;
using GitHubIntelligenceService.Domain.ValueObjects;

namespace GitHubIntelligenceService.Domain.Entities;

/// <summary>
/// Sistemizdeki ana Varlık (Aggregate Root).
/// </summary>
public class Developer : BaseEntity
{
    // Özel Alanlar (Properties)
    public string Username { get; private set; } // GitHub Kullanıcı Adı
    public string Name { get; private set; }     // Gerçek Adı
    public string? Email { get; private set; }    // E-posta (Varsa)
    public string? Bio { get; private set; }      // Biyografisi

    // İlişkiler (Relationships) - DDD Kuralları: Liste asla null olamaz, boş olabilir.
    public ICollection<RepositoryAnalysis> Repositories { get; private set; } = new List<RepositoryAnalysis>();
    
    // Değer Nesnesi (Value Object) - Puanlama Bilgisi
    public DeveloperScore Score { get; private set; }

    // Constructor ile zorunlu alanları istiyoruz.
    // Nesne oluşturulurken username zorunludur.
    public Developer(string username, string name, string? email, string? bio)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Kullanıcı adı boş olamaz.");

        Username = username;
        Name = name ?? username; // Ad yoksa kullanıcı adını ata
        Email = email;
        Bio = bio;
        
        // Başlangıçta puanı sıfır başlatıyoruz.
        Score = new DeveloperScore(0, 0, 0);
    }

    // İş Mantığı (Logic): Skoru Güncelle
    public void UpdateScore(double activity, double quality, double popularity)
    {
        Score = new DeveloperScore(activity, quality, popularity);
        UpdatedAt = DateTime.UtcNow;
    }

    // İş Mantığı: Repo Ekle
    public void AddRepository(GitHubRepository repo)
    {
        if (repo == null) throw new ArgumentNullException(nameof(repo));

        Repositories.Add(new RepositoryAnalysis(this.Id, repo));
        UpdatedAt = DateTime.UtcNow;
    }

    // --- YENİ ÖZELLİK: Teknoloji Radarı ---
    // Repoları analiz edip dil dağılımını hesaplar.
    // Örn: { "C#": 70, "Python": 30 } (Yüzde olarak)
    public Dictionary<string, double> CalculateLanguageDistribution()
    {
        var totalRepos = Repositories.Count(r => !string.IsNullOrEmpty(r.Language));
        if (totalRepos == 0) return new Dictionary<string, double>();

        return Repositories
            .Where(r => !string.IsNullOrEmpty(r.Language))
            .GroupBy(r => r.Language!)
            .ToDictionary(
                g => g.Key, 
                g => Math.Round((double)g.Count() / totalRepos * 100, 1) // Yüzde hesabı
            );
    }
}
