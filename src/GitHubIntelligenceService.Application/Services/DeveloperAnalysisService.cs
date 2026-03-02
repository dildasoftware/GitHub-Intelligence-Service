using GitHubIntelligenceService.Application.DTOs;
using GitHubIntelligenceService.Application.Interfaces;
using GitHubIntelligenceService.Domain.Entities;
using GitHubIntelligenceService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; // EKSİK OLAN BU!

namespace GitHubIntelligenceService.Application.Services;

/// <summary>
/// İş Mantığının Kalbi (Core Business Logic Implementation).
/// Application Layer, ne veritabanını bilir ne de API'yi. Sadece Interface kullanır.
/// </summary>
public class DeveloperAnalysisService : IDeveloperAnalysisService
{
    private readonly IDeveloperRepository _repository;
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<DeveloperAnalysisService> _logger;
    private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory; // YENİ: Scope Fabrikası

    public DeveloperAnalysisService(
        IDeveloperRepository repository,
        IGitHubService gitHubService,
        ILogger<DeveloperAnalysisService> logger,
        Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory)
    {
        _repository = repository;
        _gitHubService = gitHubService;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task<DeveloperDto> AnalyzeDeveloperAsync(string username, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analiz başladı: {Username}", username);

        // 1. Önce GitHub verisini çek (Infrastructure'a gitmeden Interface üzerinden)
        var userProfile = await _gitHubService.GetUserProfileAsync(username, cancellationToken);
        if (userProfile == null)
        {
            _logger.LogWarning("Kullanıcı bulunamadı: {Username}", username);
            throw new Exception($"GitHub kullanıcısı bulunamadı: {username}");
        }

        var repositories = await _gitHubService.GetUserRepositoriesAsync(username, cancellationToken);
        _logger.LogInformation("{RepoCount} adet repo bulundu.", repositories.Count);

        // 2. Domain Entity Oluştur (Business Rules burada çalışır)
        var developer = new Developer(
            userProfile.Login,
            userProfile.Name,
            userProfile.Email,
            userProfile.Bio
        );

        // 3. Repoları Ekle ve Puanla
        double totalStars = 0;
        double totalIssues = 0;

        foreach (var repo in repositories)
        {
            // Entity içindeki AddRepository metodu puanlamayı tetikler
            developer.AddRepository(repo);
            
            totalStars += repo.Stars;
            totalIssues += repo.IssuesCount;
        }

        // 4. Gelişmiş Puanlama Mantığı (Domain Logic)
        double activityScore = repositories.Count * 2; // Repository sayısı
        double popularityScore = userProfile.Followers + (totalStars * 0.5); // Takipçi + Yıldız
        double qualityScore = 100 - (totalIssues * 0.1); // Hata sayısı kaliteyi düşürür

        // Entity üzerindeki metodu kullanarak skoru güncelle
        developer.UpdateScore(activityScore, qualityScore, popularityScore);

        // 5. Veritabanına Kaydet (Upsert Yerine Insert - Tarihçe İçin)
        // Her yeni analiz yeni bir kayıt olarak eklenir. Böylece geçmiş verisi tutulur.
        // Kaydet
        await _repository.AddAsync(developer, cancellationToken);
        
        _logger.LogInformation("Analiz tamamlandı ve kaydedildi. Skor: {Score}", developer.Score.TotalScore);

        // Aktiviteleri Çek (Persona Analizi İçin)
        var activities = new List<GitHubCommitActivity>();
        try
        {
            activities = await _gitHubService.GetUserActivitiesAsync(username, cancellationToken);
        }
        catch 
        {
            // Kritik değil
        }

        // 6. DTO'ya dönüştür ve geri dön
        return MapToDto(developer, activities);
    }

    public async Task<DeveloperDto> GetAnalysisResultAsync(string username, CancellationToken cancellationToken)
    {
        // En güncel olanı getirir.
        var developer = await _repository.GetDeveloperByUsernameAsync(username, cancellationToken);
        if (developer == null) return null;

        // Tarihçe görüntülenirken de aktivite çekmeye çalışalım
        var activities = new List<GitHubCommitActivity>();
        try
        {
             activities = await _gitHubService.GetUserActivitiesAsync(username, cancellationToken);
        }
        catch { }

        return MapToDto((Developer)developer, activities); // Interface casting
    }

    // Yeni: Tarihçe Analizi
    public async Task<List<AnalysisHistoryDto>> GetAnalysisHistoryAsync(string username, CancellationToken cancellationToken)
    {
        var history = await _repository.GetDeveloperHistoryAsync(username, cancellationToken);
        
        return history.Select(d => new AnalysisHistoryDto
        {
            Date = d.UpdatedAt ?? d.CreatedAt,
            TotalScore = d.Score.TotalScore,
            QualityScore = d.Score.QualityScore,
            ActivityScore = d.Score.ActivityScore
        }).ToList();
    }

    // PARALEL (EŞZAMANLI) KARŞILAŞTIRMA - THREAD SAFE VERSION (GÜVENLİ)
    public async Task<DeveloperComparisonDto> CompareDevelopersAsync(string user1, string user2)
    {
        // Validation: Kendisiyle karşılaştırma olmaz
        if (string.Equals(user1, user2, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Aynı kullanıcıyı karşılaştıramazsınız.");
        }

        // İŞ PARÇACIĞI YÖNETİMİ (THREAD SAFETY):
        // EF Core DbContext aynı anda tek işlem yapabilir. Bu yüzden analizleri
        // izole dünyalarda (Scope) çalıştırmalıyız.
        
        var task1 = Task.Run(async () => 
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDeveloperAnalysisService>();
            return await service.AnalyzeDeveloperAsync(user1, CancellationToken.None);
        });

        var task2 = Task.Run(async () => 
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDeveloperAnalysisService>();
            return await service.AnalyzeDeveloperAsync(user2, CancellationToken.None);
        });

        // İkisi de bitene kadar bekle
        await Task.WhenAll(task1, task2);

        var dev1 = await task1;
        var dev2 = await task2;

        // 3. Kazananı Belirle
        var result = new DeveloperComparisonDto
        {
            Developer1 = dev1,
            Developer2 = dev2,
            ScoreDifference = Math.Abs(dev1.TotalScore - dev2.TotalScore),
            OverallWinner = (dev1.TotalScore > dev2.TotalScore) ? dev1.Username : 
                            (dev2.TotalScore > dev1.TotalScore) ? dev2.Username : "Berabere",
            CategoryWinners = new Dictionary<string, string>()
        };

        // Kategori Karşılaştırmaları
        result.CategoryWinners["TotalScore"] = GetWinner(dev1.Username, dev1.TotalScore, dev2.Username, dev2.TotalScore);
        result.CategoryWinners["Quality"] = GetWinner(dev1.Username, dev1.QualityScore, dev2.Username, dev2.QualityScore);
        result.CategoryWinners["Activity"] = GetWinner(dev1.Username, dev1.ActivityScore, dev2.Username, dev2.ActivityScore);
        
        return result;
    }

    private string GetWinner(string u1, double s1, string u2, double s2)
    {
        if (s1 > s2) return u1;
        if (s2 > s1) return u2;
        return "Berabere";
    }

    private static DeveloperDto MapToDto(Developer dev, List<GitHubCommitActivity> activities = null)
    {
        return new DeveloperDto
        {
            Id = dev.Id,
            Username = dev.Username,
            Name = dev.Name,
            Email = dev.Email,
            Bio = dev.Bio,
            TotalScore = dev.Score.TotalScore,
            SeniorityLevel = dev.Score.SeniorityLevel,
            ActivityScore = dev.Score.ActivityScore,
            QualityScore = dev.Score.QualityScore,
            PopularityScore = dev.Score.PopularityScore,
            LastAnalysisDate = dev.UpdatedAt ?? dev.CreatedAt,
            LanguageDistribution = dev.CalculateLanguageDistribution(), 
            Repositories = dev.Repositories.Select(r => new RepositoryDto
            {
                Name = r.Repository.Name,
                Language = r.Language,
                Stars = r.Stars,
                Issues = r.Issues,
                RepositoryScore = r.Score
            }).ToList(),

            // AI KARİYER ANALİZİ
            CareerProfile = DetermineCareerProfile(dev),

            // YENİ: Persona Analizi
            WorkHabits = AnalyzeWorkHabits(activities)
        };
    }

    // --- PERSONA ANALİZ MOTORU ---
    private static WorkHabitsDto AnalyzeWorkHabits(List<GitHubCommitActivity> activities)
    {
        if (activities == null || !activities.Any())
        {
             return new WorkHabitsDto { Persona = "Unknown Persona", Description = "Analiz için yeterli commit verisi bulunamadı.", PeakHours = "-", IsWeekendWarrior = false };
        }

        // Saat Analizi (TR Saati +3)
        var hours = activities.Select(a => a.CreatedAt.AddHours(3).Hour).ToList(); 
        
        int nightOwl = hours.Count(h => h >= 22 || h < 5);
        int earlyBird = hours.Count(h => h >= 5 && h < 10);
        int dayWalker = hours.Count(h => h >= 10 && h < 18);
        int eveningCoder = hours.Count(h => h >= 18 && h < 22);

        // Hafta Sonu Analizi
        int weekend = activities.Count(a => a.CreatedAt.DayOfWeek == DayOfWeek.Saturday || a.CreatedAt.DayOfWeek == DayOfWeek.Sunday);
        bool isWeekendWarrior = (double)weekend / activities.Count > 0.4;

        string persona = "Focused Professional 👔";
        string desc = "Standart mesai saatlerinde çalışıp iş-yaşam dengesini koruyorsunuz.";
        string peak = "09:00 - 18:00";

        int[] counts = { nightOwl, earlyBird, dayWalker, eveningCoder };
        int max = counts.Max();

        if (max == nightOwl && max > 0) 
        {
            persona = "Night Owl 🦉";
            desc = "Geceleri kod yazmak sizin için bir tutku. Sessizlikte yaratıcılığınız artıyor.";
            peak = "22:00 - 05:00";
        }
        else if (max == earlyBird && max > 0)
        {
            persona = "Early Bird 🌅";
            desc = "Güneş doğmadan klavye başına geçenlerdensiniz. Sabah dinginliği size iyi geliyor.";
            peak = "05:00 - 09:00";
        }
        else if (max == eveningCoder && max > 0)
        {
            persona = "After Hours Coder 🌇";
            desc = "İş/Okul sonrası projelere odaklanıyorsunuz. Tutkunuz mesainiz bitince başlıyor.";
            peak = "18:00 - 22:00";
        }
        else if (max == dayWalker)
        {
             persona = "Day Walker ☀️";
        }

        if (isWeekendWarrior)
        {
            persona += " + Weekend Warrior ⚔️";
            desc += " Hafta sonlarını da projelerinize ayırıyorsunuz.";
        }

        return new WorkHabitsDto
        {
            Persona = persona,
            Description = desc,
            PeakHours = peak,
            IsWeekendWarrior = isWeekendWarrior
        };
    }

    private static CareerProfileDto DetermineCareerProfile(Developer dev)
    {
        var langs = dev.CalculateLanguageDistribution();
        if (langs == null || !langs.Any()) 
        {
            return new CareerProfileDto 
            { 
                Title = "Junior Developer", 
                Description = "Henüz yeterli veri yok.", 
                SuitableRoles = new List<string> { "Intern", "Junior Developer" } 
            };
        }

        // Skor Hesapla
        double backendScore = GetCategoryScore(langs, "C#", "Java", "Go", "Rust", "C++", "PHP", "Ruby");
        double frontendScore = GetCategoryScore(langs, "JavaScript", "TypeScript", "HTML", "CSS", "Vue", "Svelte");
        double dataScore = GetCategoryScore(langs, "Python", "Jupyter Notebook", "R", "Mathematica");
        double devOpsScore = GetCategoryScore(langs, "Shell", "Dockerfile", "HCL", "Makefile");

        string title = "Full Stack Developer"; // Default
        string desc = "Çok yönlü bir geliştirici profili çiziyorsunuz.";
        var roles = new List<string>();

        // En baskın alan hangisi?
        double maxScore = Math.Max(Math.Max(backendScore, frontendScore), Math.Max(dataScore, devOpsScore));

        if (maxScore == backendScore && backendScore > 40)
        {
            title = dev.Score.SeniorityLevel == "Senior" ? "Backend Architect" : "Backend Developer";
            desc = "Sistem mimarisi ve sunucu tarafı geliştirmede güçlü bir geçmişiniz var.";
            roles.Add("Backend Engineer");
            roles.Add("API Developer");
            if(devOpsScore > 10) roles.Add("Cloud Engineer");
        }
        else if (maxScore == frontendScore && frontendScore > 40)
        {
            title = dev.Score.SeniorityLevel == "Senior" ? "Frontend Architect" : "Frontend Developer";
            desc = "Kullanıcı arayüzü ve deneyimi (UI/UX) konusunda uzmansınız.";
            roles.Add("UI Engineer");
            roles.Add("React/Vue Specialist");
        }
        else if (maxScore == dataScore && dataScore > 30)
        {
            title = "Data Scientist / AI Engineer";
            desc = "Veri analizi ve yapay zeka teknolojilerine odaklanmışsınız.";
            roles.Add("Machine Learning Engineer");
            roles.Add("Data Analyst");
        }
        else if (maxScore == devOpsScore && devOpsScore > 20)
        {
            title = "DevOps / SRE Engineer";
            desc = "Altyapı otomasyonu ve sistem yönetimi konularında yeteneklisiniz.";
            roles.Add("Platform Engineer");
            roles.Add("Cloud Architect");
        }
        else
        {
            // Dengeli Dağılım -> Full Stack
            title = "Full Stack Developer";
            desc = "Hem Frontend hem Backend geliştirebilen İsviçre Çakısı gibi bir profilsiniz.";
            roles.Add("Software Engineer");
            roles.Add("Solution Architect");
        }

        // --- Gelişim Tavsiyeleri (Smart Tips v2.0) ---
        var recommendations = new List<string>();

        // 1. DevOps & CI/CD Kontrolü
        bool hasCiCd = dev.Repositories.Any(r => r.Repository.Name.Contains(".github") || r.Repository.Name.Contains("workflow"));
        if (!hasCiCd)
        {
            recommendations.Add("🚀 **CI/CD Otomasyonu:** Projelerinde GitHub Actions veya benzeri bir CI/CD süreci göremedim. Otomasyon, modern yazılımın kalbidir.");
        }
        else if(!langs.ContainsKey("Dockerfile"))
        {
             recommendations.Add("🐳 **Containerizasyon:** Otomasyonun var ama Docker eksik. Uygulamalarını konteynerize ederek 'Benim makinemde çalışıyordu' sorununu çözebilirsin.");
        }

        // 2. Test & Kalite
        bool hasTests = dev.Repositories.Any(r => r.Repository.Name.Contains("test", StringComparison.OrdinalIgnoreCase) || r.Repository.Name.Contains("spec", StringComparison.OrdinalIgnoreCase));
        if (!hasTests)
        {
            recommendations.Add("🧪 **Test Kültürü:** TDD (Test Driven Development) yetkinliğini geliştirmelisin. Test yazılmayan kod, teknik borçtur.");
        }

        // 3. Dil Bazlı Spesifik Tavsiyeler
        if (langs.ContainsKey("JavaScript") && (!langs.ContainsKey("TypeScript") || langs["TypeScript"] < 10))
        {
            recommendations.Add("🛡️ **TypeScript'e Geçiş:** JavaScript projelerini TypeScript'e taşıyarak Tip Güvenliği (Type Safety) ve daha az runtime hatası elde edebilirsin.");
        }
        if (langs.ContainsKey("C#"))
        {
             recommendations.Add("🌐 **Modern .NET:** Backend gücünü Frontend'e taşımak için **Blazor** teknolojisine göz atabilirsin.");
        }
        if (langs.ContainsKey("Python") && !langs.ContainsKey("Jupyter Notebook"))
        {
            recommendations.Add("pandas **Veri Analizi:** Python yeteneklerini Veri Bilimi (Data Science) alanında derinleştirmek için Pandas ve NumPy kütüphanelerini incele.");
        }

        // 4. Open Source Etiği (Lisans Kontrolü)
        // (Basitçe repo isimlerinden değil ama genel bir tavsiye olarak)
        if (dev.Score.PopularityScore < 20)
        {
            recommendations.Add("📜 **Topluluk Katkısı:** Projelerine açık kaynak lisansı (MIT, Apache) eklemek ve Readme dosyalarını zenginleştirmek, GitHub'daki görünürlüğünü artırır.");
        }

        // 5. Aktivite & Disiplin
        if (dev.Score.ActivityScore < 40)
        {
            recommendations.Add("🔥 **İstikrar:** GitHub aktiviten dalgalı görünüyor. 'Zinciri Kırma' (Don't break the chain) metodunu uygulayarak her gün ufak da olsa kod yazmalısın.");
        }

        // Hiç tavsiye çıkmazsa (Mükemmel Developer!)
        if (!recommendations.Any())
        {
            recommendations.Add("👑 **Mükemmellik:** Harika bir profilin var! Artık başkalarına mentorluk yapma ve tecrübelerini blog yazılarıyla paylaşma zamanı.");
        }

        return new CareerProfileDto
        {
            Title = title,
            Description = desc,
            SuitableRoles = roles,
            Recommendations = recommendations
        };
    }

    private static double GetCategoryScore(Dictionary<string, double> dist, params string[] langs)
    {
        double sum = 0;
        foreach(var l in langs)
        {
            if(dist.ContainsKey(l)) sum += dist[l];
        }
        return sum;
    }
}
