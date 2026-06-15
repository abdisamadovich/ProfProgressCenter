using ProfProgress.Domain.Common;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// Guruh — muayyan kursning konkret oqimi (o'qituvchi, xona, jadval bilan).
/// </summary>
public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public Guid? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public DateOnly StartDate { get; set; }
    public int Capacity { get; set; } = 15;
    public string? RoomName { get; set; }

    /// <summary>Dars jadvali matni, masalan: "Du-Chor-Juma 18:00".</summary>
    public string? Schedule { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigatsiya
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}