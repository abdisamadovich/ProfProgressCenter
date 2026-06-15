using System.ComponentModel.DataAnnotations;

namespace ProfProgress.Application.Features.People;

public record StudentDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    DateOnly? BirthDate,
    string? Address,
    string? ParentPhone,
    bool IsActive,
    int ActiveEnrollments);

public record TeacherDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    string? Specialization,
    string? Bio,
    bool IsActive,
    int GroupsCount);

public record CreateTeacherRequest(
    [Required, MaxLength(150)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MaxLength(20)] string Phone,
    [Required, MinLength(6)] string Password,
    string? Specialization,
    string? Bio);