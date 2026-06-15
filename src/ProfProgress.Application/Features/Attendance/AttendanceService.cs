using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using System.Linq.Expressions;
using DomainAttendance = ProfProgress.Domain.Entities.Attendance;
using DomainLesson = ProfProgress.Domain.Entities.Lesson;

namespace ProfProgress.Application.Features.Attendance;

public class AttendanceService : IAttendanceService
{
    private readonly IAppDbContext _db;

    public AttendanceService(IAppDbContext db) => _db = db;

    public async Task<LessonDto> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct = default)
    {
        if (!await _db.Groups.AnyAsync(g => g.Id == request.GroupId, ct))
            throw new NotFoundException("Guruh", request.GroupId);

        if (request.EndTime <= request.StartTime)
            throw new AppException("Dars tugash vaqti boshlanish vaqtidan keyin bo'lishi kerak.");

        var lesson = new DomainLesson
        {
            GroupId = request.GroupId,
            Topic = request.Topic.Trim(),
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync(ct);

        return new LessonDto(lesson.Id, lesson.GroupId, lesson.Topic, lesson.Date, lesson.StartTime, lesson.EndTime);
    }

    public async Task<IReadOnlyList<LessonDto>> GetLessonsByGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        return await _db.Lessons.AsNoTracking()
            .Where(l => l.GroupId == groupId)
            .OrderByDescending(l => l.Date).ThenBy(l => l.StartTime)
            .Select(l => new LessonDto(l.Id, l.GroupId, l.Topic, l.Date, l.StartTime, l.EndTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AttendanceDto>> MarkAttendanceAsync(Guid lessonId, MarkAttendanceRequest request, CancellationToken ct = default)
    {
        var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct)
            ?? throw new NotFoundException("Dars", lessonId);

        // Faqat shu guruhga yozilgan o'quvchilar uchun davomat belgilanadi.
        var groupStudentIds = await _db.Enrollments
            .Where(e => e.GroupId == lesson.GroupId)
            .Select(e => e.StudentId)
            .ToListAsync(ct);

        var existing = await _db.Attendances
            .Where(a => a.LessonId == lessonId)
            .ToListAsync(ct);

        foreach (var item in request.Items)
        {
            if (!groupStudentIds.Contains(item.StudentId))
                throw new AppException("O'quvchi bu guruhga yozilmagan: " + item.StudentId);

            var record = existing.FirstOrDefault(a => a.StudentId == item.StudentId);
            if (record is null)
            {
                _db.Attendances.Add(new DomainAttendance
                {
                    LessonId = lessonId,
                    StudentId = item.StudentId,
                    Status = item.Status,
                    Note = item.Note
                });
            }
            else
            {
                record.Status = item.Status;
                record.Note = item.Note;
                record.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);
        return await GetAttendanceByLessonAsync(lessonId, ct);
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetAttendanceByLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        return await _db.Attendances.AsNoTracking()
            .Where(a => a.LessonId == lessonId)
            .OrderBy(a => a.Student.User.FullName)
            .Select(Projection)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetMyAttendanceAsync(Guid userId, CancellationToken ct = default)
    {
        var studentId = await _db.Students
            .Where(s => s.UserId == userId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);

        if (studentId is null)
            return Array.Empty<AttendanceDto>();

        return await _db.Attendances.AsNoTracking()
            .Where(a => a.StudentId == studentId.Value)
            .OrderByDescending(a => a.Lesson.Date)
            .Select(Projection)
            .ToListAsync(ct);
    }

    private static readonly Expression<Func<DomainAttendance, AttendanceDto>> Projection = a => new AttendanceDto(
        a.Id,
        a.LessonId,
        a.Lesson.Topic,
        a.Lesson.Date,
        a.StudentId,
        a.Student.User.FullName,
        a.Status,
        a.Note);
}