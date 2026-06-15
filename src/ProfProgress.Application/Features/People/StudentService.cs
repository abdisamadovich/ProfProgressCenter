using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using ProfProgress.Domain.Enums;
using System.Linq.Expressions;

namespace ProfProgress.Application.Features.People;

public class StudentService : IStudentService
{
    private readonly IAppDbContext _db;

    public StudentService(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(string? search, CancellationToken ct = default)
    {
        var query = _db.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x =>
                x.User.FullName.ToLower().Contains(s) ||
                x.User.Email.ToLower().Contains(s) ||
                x.User.Phone.Contains(s));
        }

        return await query
            .OrderBy(x => x.User.FullName)
            .Select(Projection)
            .ToListAsync(ct);
    }

    public async Task<StudentDto> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Students.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("O'quvchi", id);

    public async Task<StudentDto> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _db.Students.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(Projection)
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("O'quvchi profili topilmadi.");

    private static readonly Expression<Func<Student, StudentDto>> Projection = x => new StudentDto(
        x.Id,
        x.UserId,
        x.User.FullName,
        x.User.Email,
        x.User.Phone,
        x.BirthDate,
        x.Address,
        x.ParentPhone,
        x.User.IsActive,
        x.Enrollments.Count(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending));
}