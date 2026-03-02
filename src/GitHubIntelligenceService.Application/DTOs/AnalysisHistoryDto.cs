namespace GitHubIntelligenceService.Application.DTOs;

/// <summary>
/// Geçmiş analiz sonuçlarını gösteren özet DTO.
/// </summary>
public class AnalysisHistoryDto
{
    public DateTime Date { get; set; }
    public double TotalScore { get; set; }
    public double QualityScore { get; set; }
    public double ActivityScore { get; set; }
}
