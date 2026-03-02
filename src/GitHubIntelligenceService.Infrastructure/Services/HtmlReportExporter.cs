using System.Text;
using GitHubIntelligenceService.Application.DTOs;
using GitHubIntelligenceService.Application.Interfaces;

namespace GitHubIntelligenceService.Infrastructure.Services;

/// <summary>
/// Geliştirici Profilini MODERN HTML Raporu olarak çıktı veren servis.
/// v2.1 Smart Report Engine (Enhanced Design)
/// </summary>
public class HtmlReportExporter : IReportExporter
{
    public Task<(byte[] FileContent, string ContentType, string FileName)> ExportUserProfileAsync(DeveloperDto dev)
    {
        var sb = new StringBuilder();

        // --- HTML HEAD & CSS ---
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='tr'>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'>");
        sb.AppendLine($"<title>{dev.Name ?? dev.Username} - Digital Career Report</title>");
        
        // FONT: Inter
        sb.AppendLine("<link href='https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600;800&display=swap' rel='stylesheet'>");

        // MODERN CSS
        sb.AppendLine("<style>");
        sb.AppendLine(":root { --primary: #3b82f6; --secondary: #db2777; --bg: #f1f5f9; --card-bg: #ffffff; --text-dark: #1e293b; --text-light: #64748b; }");
        sb.AppendLine("body { font-family: 'Inter', sans-serif; background-color: var(--bg); color: var(--text-dark); margin: 0; padding: 40px; line-height: 1.6; }");
        sb.AppendLine(".container { max-width: 900px; margin: 0 auto; background: var(--card-bg); border-radius: 24px; box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.1); overflow: hidden; }");
        
        // Header (Gradient & Avatar Placeholder)
        sb.AppendLine(".header { background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%); color: white; padding: 60px 40px; text-align: center; position: relative; }");
        sb.AppendLine(".avatar-placeholder { width: 100px; height: 100px; background: rgba(255,255,255,0.2); border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; font-size: 3rem; font-weight: 800; border: 4px solid white; }");
        sb.AppendLine(".header h1 { margin: 0; font-size: 3rem; letter-spacing: -1px; }");
        sb.AppendLine(".header .subtitle { opacity: 0.9; margin-top: 10px; font-size: 1.2rem; font-weight: 300; }");
        
        // Grid Layout
        sb.AppendLine(".grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; padding: 40px; }");
        
        // Score Cards
        sb.AppendLine(".card { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 20px; padding: 25px; text-align: center; transition: transform 0.2s; }");
        sb.AppendLine(".card-value { font-size: 3.5rem; font-weight: 800; color: var(--primary); margin: 10px 0; line-height: 1; }");
        sb.AppendLine(".card-title { font-size: 0.85rem; text-transform: uppercase; letter-spacing: 2px; color: #94a3b8; font-weight: 700; margin-bottom: 10px; }");
        
        // AI Career Box (Highlight)
        sb.AppendLine(".ai-box { margin: 0 40px; background: linear-gradient(135deg, #fdf4ff 0%, #fae8ff 100%); border: 2px solid #f0abfc; border-radius: 20px; padding: 40px; text-align: left; }");
        sb.AppendLine(".ai-badge { display: inline-block; background: #fce7f3; color: #be185d; padding: 5px 12px; border-radius: 99px; font-size: 0.8rem; font-weight: 800; letter-spacing: 1px; margin-bottom: 10px; }");
        sb.AppendLine(".ai-title { color: #86198f; font-size: 2rem; margin: 0 0 10px 0; font-weight: 800; }");
        sb.AppendLine(".ai-desc { color: #701a75; font-size: 1.1rem; margin-bottom: 20px; }");
        sb.AppendLine(".tag { display: inline-block; background: white; color: #a21caf; padding: 8px 20px; border-radius: 12px; font-weight: 600; margin-right: 10px; border: 1px solid #e879f9; box-shadow: 0 2px 5px rgba(232, 121, 249, 0.2); }");

        // Recommendations List
        sb.AppendLine(".rec-list { list-style: none; padding: 0; margin-top: 30px; border-top: 2px dashed #f5d0fe; padding-top: 20px; }");
        sb.AppendLine(".rec-item { margin-bottom: 12px; display: flex; align-items: flex-start; gap: 12px; color: #4a044e; font-size: 1.05rem; }");
        sb.AppendLine(".bullet { color: #d946ef; font-weight: bold; font-size: 1.2rem; line-height: 1.4; }");
        sb.AppendLine(".rec-text { line-height: 1.5; }");

        // Tech Radar (Bars)
        sb.AppendLine(".section-title { font-size: 1.5rem; font-weight: 800; margin-bottom: 25px; color: var(--text-dark); display: flex; align-items: center; gap: 10px; }");
        sb.AppendLine(".bar-container { margin-bottom: 18px; }");
        sb.AppendLine(".bar-label { display: flex; justify-content: space-between; margin-bottom: 8px; font-weight: 600; font-size: 0.95rem; }");
        sb.AppendLine(".bar-bg { background: #e2e8f0; height: 12px; border-radius: 6px; overflow: hidden; }");
        sb.AppendLine(".bar-fill { height: 100%; background: var(--primary); border-radius: 6px; }");

        // Table
        sb.AppendLine("table { width: 100%; border-collapse: separate; border-spacing: 0; margin-top: 10px; }");
        sb.AppendLine("th { text-align: left; padding: 15px; border-bottom: 2px solid #e2e8f0; color: #64748b; font-weight: 700; text-transform: uppercase; font-size: 0.85rem; letter-spacing: 1px; }");
        sb.AppendLine("td { padding: 15px; border-bottom: 1px solid #f1f5f9; vertical-align: middle; }");
        sb.AppendLine(".repo-name { font-weight: 700; color: var(--text-dark); display: block; }");
        sb.AppendLine(".repo-lang { font-size: 0.85rem; color: #64748b; margin-top: 4px; display: block; }");
        sb.AppendLine(".star-badge { background: #fffbeb; color: #d97706; padding: 5px 10px; border-radius: 8px; font-weight: 700; border: 1px solid #fcd34d; font-size: 0.9rem; }");
        
        // Repo Cards (Top 10 Section)
        sb.AppendLine(".repo-section { padding: 40px; }");
        sb.AppendLine(".repo-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; margin-top: 20px; }");
        sb.AppendLine(".repo-card { background: white; border: 1px solid #e2e8f0; border-radius: 16px; padding: 20px; display: flex; flex-direction: column; gap: 10px; transition: box-shadow 0.2s; box-shadow: 0 2px 6px rgba(0,0,0,0.04); }");
        sb.AppendLine(".repo-card-name { font-weight: 800; font-size: 0.95rem; color: #1e293b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }");
        sb.AppendLine(".repo-card-badges { display: flex; flex-wrap: wrap; gap: 6px; }");
        sb.AppendLine(".badge-lang { padding: 3px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; background: #dbeafe; color: #1d4ed8; }");
        sb.AppendLine(".badge-issue { padding: 3px 10px; border-radius: 20px; font-size: 0.75rem; background: #fee2e2; color: #991b1b; }");
        sb.AppendLine(".badge-star { padding: 3px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; background: #fef9c3; color: #92400e; }");
        sb.AppendLine(".repo-card-score { font-size: 0.8rem; color: #94a3b8; margin-top: auto; display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #f1f5f9; padding-top: 10px; }");
        sb.AppendLine(".score-pill { background: linear-gradient(135deg, #6366f1, #8b5cf6); color: white; padding: 2px 10px; border-radius: 99px; font-weight: 700; font-size: 0.8rem; }");

        // Print Button
        sb.AppendLine(".print-btn { display: block; width: 200px; margin: 40px auto; padding: 15px; background: var(--text-dark); color: white; text-align: center; border-radius: 99px; text-decoration: none; font-weight: 700; cursor: pointer; transition: transform 0.2s; }");
        sb.AppendLine(".print-btn:hover { transform: translateY(-2px); box-shadow: 0 10px 20px rgba(0,0,0,0.2); }");
        sb.AppendLine("@media print { .print-btn { display: none; } body { padding: 0; background: white; } .container { box-shadow: none; max-width: 100%; margin: 0; } }");

        // Footer
        sb.AppendLine(".footer { text-align: center; padding: 40px; color: #94a3b8; font-size: 0.9rem; border-top: 1px solid #e2e8f0; margin-top: 40px; background: #f8fafc; }");
        
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // --- BODY ---
        sb.AppendLine("<div class='container'>");
        
        // 1. HEADER
        sb.AppendLine("<div class='header'>");
        string initials = dev.Username.Substring(0, 1).ToUpper();
        sb.AppendLine($"<div class='avatar-placeholder'>{initials}</div>");
        sb.AppendLine($"<h1>{dev.Name ?? dev.Username}</h1>");
        sb.AppendLine($"<div class='subtitle'>@{dev.Username} | {dev.Bio ?? "No bio available"}</div>");
        sb.AppendLine("</div>");

        // 2. SCORE CARDS
        sb.AppendLine("<div class='grid'>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Total Score</div><div class='card-value'>{dev.TotalScore:F1}</div></div>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Seniority</div><div class='card-value' style='font-size: 2.5rem; color: #10b981;'>{dev.SeniorityLevel}</div></div>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Code Quality</div><div class='card-value' style='color: #8b5cf6;'>{dev.QualityScore:F0}</div></div>");
        sb.AppendLine("</div>");

        // 3. AI CAREER INSIGHTS
        if (dev.CareerProfile != null)
        {
            sb.AppendLine("<div class='ai-box'>");
            sb.AppendLine("<span class='ai-badge'>🤖 AI CAREER COACH</span>");
            sb.AppendLine($"<div class='ai-title'>{dev.CareerProfile.Title}</div>");
            sb.AppendLine($"<p class='ai-desc'>{dev.CareerProfile.Description}</p>");
            
            sb.AppendLine("<div style='margin-bottom: 20px;'>");
            foreach(var role in dev.CareerProfile.SuitableRoles)
            {
                sb.AppendLine($"<span class='tag'>{role}</span>");
            }
            sb.AppendLine("</div>");

            // Recommendations
            if (dev.CareerProfile.Recommendations.Any())
            {
                sb.AppendLine("<ul class='rec-list'>");
                foreach(var rec in dev.CareerProfile.Recommendations)
                {
                     // Markdown tarzı bold'ları (**text**) HTML'e çevir (Basit replace)
                    string formattedRec = rec.Replace("**", "<strong>").Replace("**", "</strong>"); // Çift replace ile aç/kapa yapamayız basitçe ama sadece bold kelimeleri vurgulasak yeter.
                    // string replace yetmez, Regex lazım ama şimdilik manuel stili koruyalım.
                    // Backend'de basitçe ** kullandık. Burada HTML'de manuel temizleyelim veya direkt basalım.
                    // En temizi: Backend'de <strong> kullanalım veya burada Regex kullanalım.
                    // Hızlı Çözüm: Replace "**" with "" (temizleyelim) veya basit HTML tagleri ekleyelim.
                    
                    // Basit bir bold işleyici:
                    string htmlRec = rec;
                    if (htmlRec.Contains("**")) htmlRec = htmlRec.Replace("**", "<b>", StringComparison.Ordinal).Replace("**", "</b>", StringComparison.Ordinal); // İlkini <b> ikincisini </b> yapmaz Replace.

                    // Regex kullanmak yerine daha basit: Yıldızları temizleyelim, metin okunsun.
                    htmlRec = rec.Replace("**", ""); 

                    sb.AppendLine($"<li class='rec-item'><span class='bullet'>➜</span> <span class='rec-text'>{htmlRec}</span></li>");
                }
                sb.AppendLine("</ul>");
            }
            sb.AppendLine("</div>");
        }

        // 4. TECH RADAR & REPOS
        sb.AppendLine("<div class='grid' style='grid-template-columns: 1fr 1fr;'>");
        
        // Left: Skills
        sb.AppendLine("<div>");
        sb.AppendLine("<div class='section-title'>🚀 Top Skills</div>");
        if (dev.LanguageDistribution != null)
        {
            foreach (var lang in dev.LanguageDistribution.OrderByDescending(x => x.Value).Take(6))
            {
                sb.AppendLine("<div class='bar-container'>");
                sb.AppendLine($"<div class='bar-label'><span>{lang.Key}</span><span>{lang.Value}%</span></div>");
                sb.AppendLine($"<div class='bar-bg'><div class='bar-fill' style='width: {lang.Value}%'></div></div>");
                sb.AppendLine("</div>");
            }
        }
        sb.AppendLine("</div>");

        // Right: Top 5 Stars Summary (unchanged mini table for side-by-side)
        sb.AppendLine("<div>");
        sb.AppendLine("<div class='section-title'>⭐ Star Leaders</div>");
        sb.AppendLine("<table>");
        sb.AppendLine("<thead><tr><th>Name</th><th>Stars</th></tr></thead>");
        sb.AppendLine("<tbody>");
        foreach (var repo in dev.Repositories.OrderByDescending(r => r.Stars).Take(5))
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><span class='repo-name'>{repo.Name}</span><span class='repo-lang'>{repo.Language}</span></td>");
            sb.AppendLine($"<td><span class='star-badge'>{repo.Stars} ★</span></td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div>"); // End 2-col Grid

        // ── TOP 10 REPOSITORIES – FULL WIDTH CARD GRID ──────────────────────
        sb.AppendLine("<div class='repo-section'>");
        sb.AppendLine("<div class='section-title'>📦 Top Repositories <span style='font-size:1rem;font-weight:400;color:#94a3b8;'>— Sorted by Architectural Score</span></div>");
        sb.AppendLine("<div class='repo-grid'>");

        // Sıralama: Stars * 1.5 + RepoScore — mimari ağırlıklı
        var topRepos = dev.Repositories
            .OrderByDescending(r => r.Stars * 1.5 + r.RepositoryScore)
            .Take(10);

        int rank = 1;
        foreach (var repo in topRepos)
        {
            string rankEmoji = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : $"#{rank}";
            sb.AppendLine("<div class='repo-card'>");

            // Repo adı + sıralama
            sb.AppendLine($"<div class='repo-card-name' title='{repo.Name}'>{rankEmoji} {repo.Name}</div>");

            // Badge'ler
            sb.AppendLine("<div class='repo-card-badges'>");
            if (!string.IsNullOrEmpty(repo.Language))
                sb.AppendLine($"<span class='badge-lang'>{repo.Language}</span>");
            if (repo.Stars > 0)
                sb.AppendLine($"<span class='badge-star'>⭐ {repo.Stars}</span>");
            if (repo.Issues > 0)
                sb.AppendLine($"<span class='badge-issue'>🐛 {repo.Issues}</span>");
            sb.AppendLine("</div>");

            // Skor
            sb.AppendLine("<div class='repo-card-score'>");
            sb.AppendLine("<span>Arch. Score</span>");
            sb.AppendLine($"<span class='score-pill'>{repo.RepositoryScore:F0} pts</span>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>"); // repo-card
            rank++;
        }

        sb.AppendLine("</div>"); // repo-grid
        sb.AppendLine("</div>"); // repo-section

        // PRINT BUTTON & FOOTER
        sb.AppendLine("<a href='javascript:window.print()' class='print-btn'>🖨️ Raporu Yazdır (PDF)</a>");

        sb.AppendLine("<div class='footer'>");
        sb.AppendLine($"Generated by <strong>GitHub Intelligence Service v2.3</strong> on {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine("</div>");

        sb.AppendLine("</div>"); // End Container
        sb.AppendLine("</body></html>");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Task.FromResult((bytes, "text/html", $"{dev.Username}_CareerReport.html"));
    }
}
