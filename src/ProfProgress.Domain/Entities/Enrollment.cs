using ProfProgress.Domain.Common;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// O'quvchining guruhga yozilishi (many-to-many bog'lovchi entity).
/// </summary>
public class Enrollment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
}