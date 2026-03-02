using GitHubIntelligenceService.Domain.Entities;
using GitHubIntelligenceService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GitHubIntelligenceService.Infrastructure.Persistence;

public class DeveloperRepository : IDeveloperRepository
{
    private readonly AppDbContext _context;

    public DeveloperRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Developer?> GetDeveloperByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        // En GÜNCEL analizi getir
        return await _context.Developers
            .Include(d => d.Repositories)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt) 
            .FirstOrDefaultAsync(d => d.Username == username, cancellationToken);
    }
    
    public async Task<List<Developer>> GetDeveloperHistoryAsync(string username, CancellationToken cancellationToken)
    {
        // Tüm geçmişi getir (Tarihe göre ESKİDEN YENİYE sıralı - Grafik çizimi için)
        // Repoları dahil etmeye gerek yok, sadece skor ve tarih lazım. (Performans Optimizasyonu)
        return await _context.Developers
            .Where(d => d.Username == username)
            .OrderBy(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Developer developer, CancellationToken cancellationToken)
    {
        await _context.Developers.AddAsync(developer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Developer developer, CancellationToken cancellationToken)
    {
        _context.Developers.Update(developer);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
