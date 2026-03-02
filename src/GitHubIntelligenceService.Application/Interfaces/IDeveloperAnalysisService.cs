using GitHubIntelligenceService.Application.DTOs;

namespace GitHubIntelligenceService.Application.Interfaces;

/// <summary>
/// Projenin asıl iş mantığını (Business Logic) çalıştıran servis arayüzü.
/// API Controller burayı çağırır.
/// </summary>
public interface IDeveloperAnalysisService
{
    /// <summary>
    /// Bir geliştiricinin tüm GitHub verisini çeker, analiz eder, puanlar ve kaydeder.
    /// Sonuç olarak DTO döner.
    /// </summary>
    Task<DeveloperDto> AnalyzeDeveloperAsync(string username, CancellationToken cancellationToken);
    
    /// <summary>
    /// Veritabanında kayıtlı olan analiz sonuçlarını getirir.
    /// </summary>
    Task<DeveloperDto> GetAnalysisResultAsync(string username, CancellationToken cancellationToken);
    
    /// <summary>
    /// Geliştiricinin geçmiş analiz sonuçlarını tarih sırasına göre getirir.
    /// </summary>
    Task<List<AnalysisHistoryDto>> GetAnalysisHistoryAsync(string username, CancellationToken cancellationToken);

    // Karşılaştırma Analizi
    Task<DeveloperComparisonDto> CompareDevelopersAsync(string user1, string user2);
}
