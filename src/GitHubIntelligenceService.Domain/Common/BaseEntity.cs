namespace GitHubIntelligenceService.Domain.Common;

/// <summary>
/// Temel Varlık Sınıfı. Tüm domain varlıkları (Entity) bu sınıftan türeyecek.
/// Bu sınıf, tüm veritabanı tablolarında ortak olan alanları içerir.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Her kaydın benzersiz kimliği (Primary Key).
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Kaydın oluşturulma tarihi.
    /// </summary>
    public DateTime CreatedAt { get;  set; }

    /// <summary>
    /// Kaydın son güncellenme tarihi.
    /// </summary>
    public DateTime? UpdatedAt { get;  set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
