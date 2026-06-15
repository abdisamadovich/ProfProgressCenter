using ProfProgress.Domain.Common;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// O'quvchi profili. User bilan 1:1 bog'langan.
/// </summary>
public class Student : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }
    public string? Address { get; set; }
    public string? ParentPhone { get; set; }

    // Navigatsiya
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}