using GitHubIntelligenceService.Domain.Common;

namespace GitHubIntelligenceService.Domain.Entities;

/// <summary>
/// Bir Repo'nun analiz sonuçlarını tutan Entity.
/// İlişki: Bir Developer'ın N tane RepositoryAnalysis'i olur (1-N).
/// </summary>
public class RepositoryAnalysis : BaseEntity
{
    // İlişki (Foreign Key)
    public Guid DeveloperId { get; private set; }
    
    // Gömülü Nesne (Embeddable) olarak tasarlanabilir ama Entity yapıyoruz.
    public GitHubRepository Repository { get; private set; }

    public double Score { get; private set; } // Bu reponun puanı
    public DateTime AnalysisDate { get; private set; } // Analiz Tarihi
    public string? Language { get; private set; } // Ana Dil
    public int Stars { get; private set; } // Yıldız Sayısı
    public int Issues { get; private set; } // Açık Hata Sayısı

    // Constructor ile zorunlu alanları istiyoruz.
    public RepositoryAnalysis(Guid developerId, GitHubRepository repository)
    {
        DeveloperId = developerId;
        Repository = repository;
        AnalysisDate = DateTime.UtcNow;
        Language = repository.Language;
        Stars = repository.Stars;
        Issues = repository.IssuesCount;

        CalculateRepoScore();
    }

    // EF Core için boş constructor (Oluştururken hata almaması için)
    protected RepositoryAnalysis() { }

    // İş Mantığı: Repo Puanını Hesapla
    private void CalculateRepoScore()
    {
        // Örnek: Yıldız başına 2 puan, Issue başına -1 puan
        Score = (Stars * 2) - (Issues * 1);
        if (Score < 0) Score = 0; // Puan negatif olamaz
    }
}

/// <summary>
/// GitHub'dan gelen ham repo bilgisi.
/// </summary>
public class GitHubRepository
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public int Stars { get; set; }
    public int Forks { get; set; }
    public int IssuesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastPushedAt { get; set; }
}

