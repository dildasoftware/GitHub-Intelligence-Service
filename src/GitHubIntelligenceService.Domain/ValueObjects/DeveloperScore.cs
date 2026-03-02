using GitHubIntelligenceService.Domain.Common;

namespace GitHubIntelligenceService.Domain.ValueObjects;

/// <summary>
/// Geliştiricinin hesaplanan skorunu temsil eden Value Object.
/// Bu nesne değişmezdir (Immutable).
/// </summary>
public class DeveloperScore
{
    /// <summary>
    /// Toplam Puan (0-100 arası).
    /// </summary>
    public double TotalScore { get; private set; }

    /// <summary>
    /// Aktivite Puanı (Commit sıklığı).
    /// </summary>
    public double ActivityScore { get; private set; }

    /// <summary>
    /// Kalite Puanı (Test, Dokümantasyon).
    /// </summary>
    public double QualityScore { get; private set; }

    /// <summary>
    /// Popülerlik Puanı (Stars, Followers).
    /// </summary>
    public double PopularityScore { get; private set; }

    /// <summary>
    /// Hesaplanan Skora göre Kıdem Seviyesi (Junior, Mid, Senior, Expert).
    /// </summary>
    public string SeniorityLevel { get; private set; }

    // Boş constructor EF Core ve Serialization için gerekli olabilir.
    public DeveloperScore() { }

    /// <summary>
    /// Yeni bir skor oluşturur.
    /// Kurallar burada işletilir: Puanlar 0-100 arasında olmalıdır.
    /// </summary>
    public DeveloperScore(double activity, double quality, double popularity)
    {
        if (activity < 0 || quality < 0 || popularity < 0)
            throw new ArgumentException("Puanlar negatif olamaz.");

        ActivityScore = Math.Min(activity, 100);
        QualityScore = Math.Min(quality, 100);
        PopularityScore = Math.Min(popularity, 100);

        CalculateTotal();
        CalculateSeniority();
    }

    private void CalculateTotal()
    {
        // Örnek Formül: Aktivite %40, Kalite %40, Popülerlik %20
        TotalScore = (ActivityScore * 0.4) + (QualityScore * 0.4) + (PopularityScore * 0.2);
    }

    private void CalculateSeniority()
    {
        SeniorityLevel = TotalScore switch
        {
            >= 85 => "Expert (Uzman)",
            >= 65 => "Senior (Kıdemli)",
            >= 40 => "Mid-Level (Orta)",
            _ => "Junior (Başlangıç)"
        };
    }
}
