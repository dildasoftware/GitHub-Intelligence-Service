using GitHubIntelligenceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GitHubIntelligenceService.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Developer> Developers { get; set; }
    public DbSet<RepositoryAnalysis> Repositories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Value Object Mapping (DeveloperScore)
        modelBuilder.Entity<Developer>().OwnsOne(d => d.Score);

        // Repo ilişkisi
        modelBuilder.Entity<RepositoryAnalysis>()
            .HasKey(r => r.Id);

        // GitHubRepository bir entity değil, bir Value Object gibi davranmalı (Owned Type)
        // Default convention ile kolon isimleri Repository_Name vs. olacak.
        modelBuilder.Entity<RepositoryAnalysis>()
            .OwnsOne(r => r.Repository);

        // Developer ile RepositoryAnalysis arasındaki ilişki
        modelBuilder.Entity<Developer>()
            .HasMany(d => d.Repositories)
            .WithOne()
            .HasForeignKey(r => r.DeveloperId);
    }
}
