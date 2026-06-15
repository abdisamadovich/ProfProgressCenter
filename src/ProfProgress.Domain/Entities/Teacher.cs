using ProfProgress.Domain.Common;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// O'qituvchi profili. User bilan 1:1 bog'langan.
/// </summary>
public class Teacher : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Specialization { get; set; }
    public string? Bio { get; set; }

    // Navigatsiya
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}