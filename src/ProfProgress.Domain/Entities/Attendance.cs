using ProfProgress.Domain.Common;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// Bitta o'quvchining bitta darsdagi davomati.
/// </summary>
public class Attendance : BaseEntity
{
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Note { get; set; }
}