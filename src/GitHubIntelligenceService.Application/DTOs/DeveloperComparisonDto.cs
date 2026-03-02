namespace GitHubIntelligenceService.Application.DTOs;

public class DeveloperComparisonDto
{
    public DeveloperDto Developer1 { get; set; } = null!;
    public DeveloperDto Developer2 { get; set; } = null!;
    
    // Kazananın Kullanıcı Adı
    public string OverallWinner { get; set; } = string.Empty;
    
    // Puan Farkı
    public double ScoreDifference { get; set; }

    // Kategori Bazlı Kazananlar (Örn: "Kalite": "Dev1", "Hız": "Dev2")
    public Dictionary<string, string> CategoryWinners { get; set; } = new();
}
