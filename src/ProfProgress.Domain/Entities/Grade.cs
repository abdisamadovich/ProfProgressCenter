using ProfProgress.Domain.Common;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// O'quvchining bahosi/natijasi (uy vazifasi, test, imtihon va h.k.).
/// </summary>
public class Grade : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    /// <summary>Ixtiyoriy — agar baho aniq bir darsga tegishli bo'lsa.</summary>
    public Guid? LessonId { get; set; }

    public Lesson? Lesson { get; set; }

    public GradeType Type { get; set; } = GradeType.Homework;
    public double Score { get; set; }
    public double MaxScore { get; set; } = 100;
    public string? Comment { get; set; }
    public DateOnly Date { get; set; }
}