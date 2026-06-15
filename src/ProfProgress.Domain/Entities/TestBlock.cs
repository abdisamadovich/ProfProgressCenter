using ProfProgress.Domain.Common;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// Test bloki (DTM uslubi). Har bir blokdagi savol uchun beriladigan ball turlicha:
/// masalan 1-blok ×1.1, 2-blok ×2.1, 3-blok ×3.1.
/// </summary>
public class TestBlock : BaseEntity
{
    public Guid TestId { get; set; }
    public Test Test { get; set; } = null!;

    public string? Name { get; set; }

    /// <summary>Blok savollarining boshlanish raqami (1 dan).</summary>
    public int FromQuestion { get; set; }

    /// <summary>Blok savollarining tugash raqami (shu raqam ham kiradi).</summary>
    public int ToQuestion { get; set; }

    /// <summary>Shu blokdagi har bir to'g'ri javob uchun ball.</summary>
    public decimal PointsPerQuestion { get; set; } = 1m;
}
