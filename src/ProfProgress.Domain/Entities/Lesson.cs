using ProfProgress.Domain.Common;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// Guruhdagi bitta dars (sana va vaqt bilan). Davomat shu darsga bog'lanadi.
/// </summary>
public class Lesson : BaseEntity
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public string Topic { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    // Navigatsiya
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}