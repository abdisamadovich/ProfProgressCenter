using ProfProgress.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProfProgress.Application.Features.Enrollments;

/// <summary>O'quvchining o'zi yozilishi uchun (StudentId joriy userdan olinadi).</summary>
public record EnrollRequest(
    [Required] Guid GroupId);

/// <summary>Admin tomonidan boshqa o'quvchini yozish uchun.</summary>
public record AdminEnrollRequest(
    [Required] Guid StudentId,
    [Required] Guid GroupId);

public record UpdateEnrollmentStatusRequest(
    [Required] EnrollmentStatus Status);

public record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid GroupId,
    string GroupName,
    string CourseTitle,
    DateTime EnrolledAt,
    EnrollmentStatus Status);