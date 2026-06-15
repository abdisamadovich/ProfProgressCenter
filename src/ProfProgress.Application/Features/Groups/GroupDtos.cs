using System.ComponentModel.DataAnnotations;

namespace ProfProgress.Application.Features.Groups;

public record GroupDto(
    Guid Id,
    string Name,
    Guid CourseId,
    string CourseTitle,
    Guid? TeacherId,
    string? TeacherName,
    DateOnly StartDate,
    int Capacity,
    int EnrolledCount,
    string? RoomName,
    string? Schedule,
    bool IsActive);

public record CreateGroupRequest(
    [Required, MaxLength(150)] string Name,
    [Required] Guid CourseId,
    Guid? TeacherId,
    DateOnly StartDate,
    [Range(1, 200)] int Capacity,
    string? RoomName,
    string? Schedule);

public record UpdateGroupRequest(
    [Required, MaxLength(150)] string Name,
    [Required] Guid CourseId,
    Guid? TeacherId,
    DateOnly StartDate,
    [Range(1, 200)] int Capacity,
    string? RoomName,
    string? Schedule,
    bool IsActive);