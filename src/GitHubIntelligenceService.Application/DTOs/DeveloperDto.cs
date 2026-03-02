using GitHubIntelligenceService.Domain.Entities;

namespace GitHubIntelligenceService.Application.DTOs;

/// <summary>
/// API'den veya ekrandan kullanıcıya döneceğimiz veri.
/// Domain Entity'sini (Developer) direkt dışarı açmıyoruz.
/// </summary>
public class DeveloperDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Bio { get; set; }
    
    // DeveloperScore içindeki değerleri düzleştiriyoruz (Flattening)
    public double TotalScore { get; set; }
    public string SeniorityLevel { get; set; }
    public double ActivityScore { get; set; }
    public double QualityScore { get; set; }
    public double PopularityScore { get; set; }

    public DateTime LastAnalysisDate { get; set; }
    
    // YENİ: Teknoloji Radarı (Hangi dilden yüzde kaç?)
    public Dictionary<string, double>? LanguageDistribution { get; set; }
    
    // YENİ: Kariyer Profili (AI Önerisi)
    public CareerProfileDto? CareerProfile { get; set; }

    public List<RepositoryDto> Repositories { get; set; } = new();

    // YENİ: Alışkanlıklar (Persona)
    public WorkHabitsDto? WorkHabits { get; set; }
}

public class WorkHabitsDto
{
    public string Persona { get; set; } // Örn: "Night Owl"
    public string Description { get; set; } // Örn: "Gecelerin Efendisi"
    public string PeakHours { get; set; } // Örn: "23:00 - 02:00"
    public bool IsWeekendWarrior { get; set; } // Hafta sonu çalışıyor mu?
}

public class CareerProfileDto
{
    public string Title { get; set; } = string.Empty; // Örn: Senior Backend Developer
    public string Description { get; set; } = string.Empty; // Neden bu rol?
    public List<string> SuitableRoles { get; set; } = new(); // Alternatif roller
    public List<string> Recommendations { get; set; } = new(); // YENİ: Gelişim Tavsiyeleri
}

public class RepositoryDto
{
    public string Name { get; set; }
    public string Language { get; set; }
    public int Stars { get; set; }
    public int Issues { get; set; }
    public double RepositoryScore { get; set; }
}
