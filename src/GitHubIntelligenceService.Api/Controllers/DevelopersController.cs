using GitHubIntelligenceService.Application.DTOs;
using GitHubIntelligenceService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GitHubIntelligenceService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevelopersController : ControllerBase
{
    private readonly IDeveloperAnalysisService _analysisService;
    private readonly ILogger<DevelopersController> _logger;

    private readonly IReportExporter _reportExporter;

    public DevelopersController(
        IDeveloperAnalysisService analysisService,
        IReportExporter reportExporter,
        ILogger<DevelopersController> logger)
    {
        _analysisService = analysisService;
        _reportExporter = reportExporter;
        _logger = logger;
    }

    /// <summary>
    /// Var olan analiz sonucunu getirir.
    /// </summary>
    /// <param name="username">GitHub kullanıcı adı</param>
    /// <returns>Developer Analiz Raporu</returns>
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(DeveloperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string username, CancellationToken cancellationToken)
    {
        var result = await _analysisService.GetAnalysisResultAsync(username, cancellationToken);
        
        if (result == null)
        {
            return NotFound(new { Message = "Analiz bulunamadı. Lütfen önce analiz başlatın." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Yeni bir analiz başlatır.
    /// GitHub'a gider, veriyi çeker, puanlar ve kaydeder.
    /// </summary>
    /// <param name="username">GitHub kullanıcı adı</param>
    /// <returns>Yeni oluşturulan analiz raporu</returns>
    [HttpPost("{username}/analyze")]
    [ProducesResponseType(typeof(DeveloperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Analyze(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { Message = "Kullanıcı adı boş olamaz." });
        }

        try
        {
            var result = await _analysisService.AnalyzeDeveloperAsync(username, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analiz sırasında hata oluştu: {Username}", username);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Analiz raporunu HTML dosyası olarak indirir.
    /// </summary>
    /// <param name="username">GitHub kullanıcı adı</param>
    /// <returns>HTML Dosyası</returns>
    [HttpGet("{username}/export")]
    public async Task<IActionResult> Export(string username, CancellationToken cancellationToken)
    {
        // 1. Önce veriyi al
        var developer = await _analysisService.GetAnalysisResultAsync(username, cancellationToken);
        if (developer == null)
        {
            return NotFound(new { Message = "Rapor alınamadı çünkü kullanıcı henüz analiz edilmemiş." });
        }

        // 2. Raporu oluştur (HTML byte dizisi)
        var report = await _reportExporter.ExportUserProfileAsync(developer);

        // 3. Dosyayı tarayıcıda aç (inline) — yeni sekmede HTML olarak görünsün
        //    'attachment' kullanırsak indiriyor ama Windows hangi uygulamayla açacağını bilmiyor.
        //    'inline' ile tarayıcı direkt aynı/yeni sekmede gösteriyor.
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{report.FileName}\"";
        return File(report.FileContent, report.ContentType);
    }

    /// <summary>
    /// Geliştiricinin geçmiş analizlerini getirir.
    /// </summary>
    [HttpGet("{username}/history")]
    [ProducesResponseType(typeof(List<AnalysisHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(string username, CancellationToken cancellationToken)
    {
        var history = await _analysisService.GetAnalysisHistoryAsync(username, cancellationToken);
        if (history == null || !history.Any())
        {
             return NotFound(new { Message = "Geçmiş analiz verisi bulunamadı." });
        }
        return Ok(history);
    }

    /// <summary>
    /// İki geliştiriciyi karşılaştırır.
    /// </summary>
    /// <param name="user1">İlk Kullanıcı</param>
    /// <param name="user2">İkinci Kullanıcı</param>
    /// <returns>Karşılaştırma Sonucu</returns>
    [HttpGet("compare")]
    [ProducesResponseType(typeof(DeveloperComparisonDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Compare([FromQuery] string user1, [FromQuery] string user2)
    {
        if (string.IsNullOrWhiteSpace(user1) || string.IsNullOrWhiteSpace(user2))
        {
            return BadRequest(new { Message = "Her iki kullanıcı adı da girilmelidir." });
        }

        try
        {
            var result = await _analysisService.CompareDevelopersAsync(user1, user2);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
