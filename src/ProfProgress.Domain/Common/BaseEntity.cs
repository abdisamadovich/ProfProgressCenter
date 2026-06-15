namespace ProfProgress.Domain.Common;

/// <summary>
/// Barcha entity'lar uchun umumiy asos (Id va audit maydonlari).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}