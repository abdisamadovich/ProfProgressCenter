using ProfProgress.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProfProgress.Application.Features.Attendance;

/// <summary>Dars yaratish so'rovi.</summary>
public record CreateLessonRequest(
    [Required] Guid GroupId,
    [Required, MaxLength(250)] string Topic,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime);

public record LessonDto(
    Guid Id,
    Guid GroupId,
    string Topic,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime);

public record AttendanceItem(
    [Required] Guid StudentId,
    AttendanceStatus Status,
    string? Note);

/// <summary>Bitta dars uchun butun guruh davomatini belgilash.</summary>
public record MarkAttendanceRequest(
    [Required] List<AttendanceItem> Items);

public record AttendanceDto(
    Guid Id,
    Guid LessonId,
    string LessonTopic,
    DateOnly LessonDate,
    Guid StudentId,
    string StudentName,
    AttendanceStatus Status,
    string? Note);