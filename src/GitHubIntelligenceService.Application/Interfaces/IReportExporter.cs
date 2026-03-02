using GitHubIntelligenceService.Application.DTOs;

namespace GitHubIntelligenceService.Application.Interfaces;

/// <summary>
/// Raporlama Servisi Arayüzü.
/// Farklı formatlarda (HTML, PDF, Excel) rapor almak için genişletilebilir.
/// </summary>
public interface IReportExporter
{
    /// <summary>
    /// Verilen geliştirici verisini rapor dosyasına dönüştürür.
    /// </summary>
    /// <returns>Dosya içeriği (byte array) ve Content-Type (MIME type)</returns>
    Task<(byte[] FileContent, string ContentType, string FileName)> ExportUserProfileAsync(DeveloperDto developer);
}
