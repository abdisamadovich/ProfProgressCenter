using ProfProgress.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProfProgress.Application.Features.Grades;

public record CreateGradeRequest(
    [Required] Guid StudentId,
    [Required] Guid GroupId,
    Guid? LessonId,
    GradeType Type,
    [Range(0, 1000)] double Score,
    [Range(1, 1000)] double MaxScore,
    string? Comment,
    DateOnly Date);

public record GradeDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid GroupId,
    string GroupName,
    GradeType Type,
    double Score,
    double MaxScore,
    string? Comment,
    DateOnly Date);