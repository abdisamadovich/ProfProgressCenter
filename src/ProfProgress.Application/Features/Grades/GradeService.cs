using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using System.Linq.Expressions;

namespace ProfProgress.Application.Features.Grades;

public class GradeService : IGradeService
{
    private readonly IAppDbContext _db;

    public GradeService(IAppDbContext db) => _db = db;

    public async Task<GradeDto> CreateAsync(CreateGradeRequest request, CancellationToken ct = default)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == request.StudentId, ct))
            throw new NotFoundException("O'quvchi", request.StudentId);

        if (!await _db.Groups.AnyAsync(g => g.Id == request.GroupId, ct))
            throw new NotFoundException("Guruh", request.GroupId);

        if (request.Score > request.MaxScore)
            throw new AppException("Olingan ball maksimal balldan katta bo'lishi mumkin emas.");

        var grade = new Grade
        {
            StudentId = request.StudentId,
            GroupId = request.GroupId,
            LessonId = request.LessonId,
            Type = request.Type,
            Score = request.Score,
            MaxScore = request.MaxScore,
            Comment = request.Comment,
            Date = request.Date
        };

        _db.Grades.Add(grade);
        await _db.SaveChangesAsync(ct);

        return await _db.Grades.AsNoTracking()
            .Where(g => g.Id == grade.Id)
            .Select(Projection)
            .FirstAsync(ct);
    }

    public async Task<IReadOnlyList<GradeDto>> GetByGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        return await _db.Grades.AsNoTracking()
            .Where(g => g.GroupId == groupId)
            .OrderByDescending(g => g.Date)
            .Select(Projection)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GradeDto>> GetMyGradesAsync(Guid userId, CancellationToken ct = default)
    {
        var studentId = await _db.Students
            .Where(s => s.UserId == userId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);

        if (studentId is null)
            return Array.Empty<GradeDto>();

        return await _db.Grades.AsNoTracking()
            .Where(g => g.StudentId == studentId.Value)
            .OrderByDescending(g => g.Date)
            .Select(Projection)
            .ToListAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new NotFoundException("Baho", id);

        _db.Grades.Remove(grade);
        await _db.SaveChangesAsync(ct);
    }

    private static readonly Expression<Func<Grade, GradeDto>> Projection = g => new GradeDto(
        g.Id,
        g.StudentId,
        g.Student.User.FullName,
        g.GroupId,
        g.Group.Name,
        g.Type,
        g.Score,
        g.MaxScore,
        g.Comment,
        g.Date);
}