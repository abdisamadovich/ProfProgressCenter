using ProfProgress.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProfProgress.Application.Features.Courses;

public record CourseDto(
    Guid Id,
    string Title,
    string? Description,
    decimal Price,
    int DurationMonths,
    CourseLevel Level,
    bool IsActive,
    int GroupsCount);

public record CreateCourseRequest(
    [Required, MaxLength(200)] string Title,
    string? Description,
    [Range(0, 1_000_000_000)] decimal Price,
    [Range(1, 60)] int DurationMonths,
    CourseLevel Level);

public record UpdateCourseRequest(
    [Required, MaxLength(200)] string Title,
    string? Description,
    [Range(0, 1_000_000_000)] decimal Price,
    [Range(1, 60)] int DurationMonths,
    CourseLevel Level,
    bool IsActive);