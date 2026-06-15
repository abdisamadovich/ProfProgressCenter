using ProfProgress.Domain.Common;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// Kurs (yo'nalish). Masalan: "Frontend dasturlash", "Ingliz tili".
/// </summary>
public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMonths { get; set; }
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    public bool IsActive { get; set; } = true;

    // Navigatsiya
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}