using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using System.Linq.Expressions;

namespace ProfProgress.Application.Features.Groups;

public class GroupService : IGroupService
{
    private readonly IAppDbContext _db;

    public GroupService(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<GroupDto>> GetAllAsync(Guid? courseId, CancellationToken ct = default)
    {
        var query = _db.Groups.AsNoTracking();
        if (courseId.HasValue)
            query = query.Where(g => g.CourseId == courseId.Value);

        return await query
            .OrderByDescending(g => g.StartDate)
            .Select(Projection)
            .ToListAsync(ct);
    }

    public async Task<GroupDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Groups.AsNoTracking()
            .Where(g => g.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Guruh", id);
    }

    public async Task<GroupDto> CreateAsync(CreateGroupRequest request, CancellationToken ct = default)
    {
        await EnsureCourseExists(request.CourseId, ct);
        await EnsureTeacherExists(request.TeacherId, ct);

        var group = new Group
        {
            Name = request.Name.Trim(),
            CourseId = request.CourseId,
            TeacherId = request.TeacherId,
            StartDate = request.StartDate,
            Capacity = request.Capacity,
            RoomName = request.RoomName,
            Schedule = request.Schedule,
            IsActive = true
        };

        _db.Groups.Add(group);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(group.Id, ct);
    }

    public async Task<GroupDto> UpdateAsync(Guid id, UpdateGroupRequest request, CancellationToken ct = default)
    {
        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new NotFoundException("Guruh", id);

        await EnsureCourseExists(request.CourseId, ct);
        await EnsureTeacherExists(request.TeacherId, ct);

        group.Name = request.Name.Trim();
        group.CourseId = request.CourseId;
        group.TeacherId = request.TeacherId;
        group.StartDate = request.StartDate;
        group.Capacity = request.Capacity;
        group.RoomName = request.RoomName;
        group.Schedule = request.Schedule;
        group.IsActive = request.IsActive;
        group.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(group.Id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var group = await _db.Groups
            .Include(g => g.Enrollments)
            .FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new NotFoundException("Guruh", id);

        if (group.Enrollments.Count > 0)
            throw new ConflictException("Guruhda o'quvchilar mavjud. Avval o'quvchilarni chiqaring.");

        _db.Groups.Remove(group);
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureCourseExists(Guid courseId, CancellationToken ct)
    {
        if (!await _db.Courses.AnyAsync(c => c.Id == courseId, ct))
            throw new NotFoundException("Kurs", courseId);
    }

    private async Task EnsureTeacherExists(Guid? teacherId, CancellationToken ct)
    {
        if (teacherId.HasValue && !await _db.Teachers.AnyAsync(t => t.Id == teacherId.Value, ct))
            throw new NotFoundException("O'qituvchi", teacherId.Value);
    }

    // EF Core SQL'ga tarjima qiladigan projeksiya ifodasi.
    private static readonly Expression<Func<Group, GroupDto>> Projection = g => new GroupDto(
        g.Id,
        g.Name,
        g.CourseId,
        g.Course.Title,
        g.TeacherId,
        g.Teacher != null ? g.Teacher.User.FullName : null,
        g.StartDate,
        g.Capacity,
        g.Enrollments.Count,
        g.RoomName,
        g.Schedule,
        g.IsActive);
}