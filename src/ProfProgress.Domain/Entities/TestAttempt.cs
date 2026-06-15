using ProfProgress.Domain.Common;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// Talabaning testga bergan javoblari va natijasi.
/// </summary>
public class TestAttempt : BaseEntity
{
    public Guid TestId { get; set; }
    public Test Test { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Talaba yuborgan javoblar (normallashtirilgan, masalan "ABCDA...").</summary>
    public string Answers { get; set; } = string.Empty;

    public int CorrectCount { get; set; }
    public decimal TotalScore { get; set; }

    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;
}
