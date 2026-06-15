using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using ProfProgress.Domain.Enums;
using System.Linq.Expressions;

namespace ProfProgress.Application.Features.Enrollments;

public class EnrollmentService : IEnrollmentService
{
    private readonly IAppDbContext _db;

    public EnrollmentService(IAppDbContext db) => _db = db;

    public async Task<EnrollmentDto> EnrollSelfAsync(Guid userId, EnrollRequest request, CancellationToken ct = default)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? throw new NotFoundException("Sizning o'quvchi profilingiz topilmadi.");

        return await CreateEnrollmentAsync(student.Id, request.GroupId, ct);
    }

    public async Task<EnrollmentDto> AdminEnrollAsync(AdminEnrollRequest request, CancellationToken ct = default)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == request.StudentId, ct))
            throw new NotFoundException("O'quvchi", request.StudentId);

        return await CreateEnrollmentAsync(request.StudentId, request.GroupId, ct);
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetMyEnrollmentsAsync(Guid userId, CancellationToken ct = default)
    {
        var studentId = await _db.Students
            .Where(s => s.UserId == userId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);

        if (studentId is null)
            return Array.Empty<EnrollmentDto>();

        return await _db.Enrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId.Value)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(Projection)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetByGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        return await _db.Enrollments.AsNoTracking()
            .Where(e => e.GroupId == groupId)
            .OrderBy(e => e.Student.User.FullName)
            .Select(Projection)
            .ToListAsync(ct);
    }

    public async Task<EnrollmentDto> UpdateStatusAsync(Guid enrollmentId, UpdateEnrollmentStatusRequest request, CancellationToken ct = default)
    {
        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, ct)
            ?? throw new NotFoundException("Yozilish", enrollmentId);

        enrollment.Status = request.Status;
        enrollment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(enrollment.Id, ct);
    }

    // --- yordamchi ---

    private async Task<EnrollmentDto> CreateEnrollmentAsync(Guid studentId, Guid groupId, CancellationToken ct)
    {
        var group = await _db.Groups
            .Include(g => g.Enrollments)
            .FirstOrDefaultAsync(g => g.Id == groupId, ct)
            ?? throw new NotFoundException("Guruh", groupId);

        if (!group.IsActive)
            throw new ConflictException("Bu guruh faol emas.");

        var alreadyEnrolled = group.Enrollments.Any(e =>
            e.StudentId == studentId && e.Status != EnrollmentStatus.Cancelled);
        if (alreadyEnrolled)
            throw new ConflictException("O'quvchi bu guruhga allaqachon yozilgan.");

        var activeCount = group.Enrollments.Count(e =>
            e.Status is EnrollmentStatus.Pending or EnrollmentStatus.Active);
        if (activeCount >= group.Capacity)
            throw new ConflictException("Guruh to'lgan, bo'sh joy yo'q.");

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            GroupId = groupId,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Pending
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(enrollment.Id, ct);
    }

    private async Task<EnrollmentDto> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Enrollments.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(Projection)
            .FirstAsync(ct);

    private static readonly Expression<Func<Enrollment, EnrollmentDto>> Projection = e => new EnrollmentDto(
        e.Id,
        e.StudentId,
        e.Student.User.FullName,
        e.GroupId,
        e.Group.Name,
        e.Group.Course.Title,
        e.EnrolledAt,
        e.Status);
}