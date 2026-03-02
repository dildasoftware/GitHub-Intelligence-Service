using GitHubIntelligenceService.Domain.Entities;

namespace GitHubIntelligenceService.Domain.Interfaces;

/// <summary>
/// Geliştiricilerin kaydedilmesi ve sorgulanması için gerekli arayüz.
/// Veritabanına nasıl bağlanacağını uygulayan Infrastructure katmanı belirleyecek.
/// </summary>
public interface IDeveloperRepository
{
    // CancellationToken: Uzun süren işlemleri iptal etmek için asenkron yapı.
    Task<Developer?> GetDeveloperByUsernameAsync(string username, CancellationToken cancellationToken);
    
    // Geliştirici Tarihçesi (Zaman İçindeki Değişim Analizi İçin)
    Task<List<Developer>> GetDeveloperHistoryAsync(string username, CancellationToken cancellationToken);
    
    // Developer güncelleme
    Task UpdateAsync(Developer developer, CancellationToken cancellationToken);

    // Yeni Developer Ekle
    Task AddAsync(Developer developer, CancellationToken cancellationToken);
}
