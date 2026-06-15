using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;

namespace ProfProgress.Application.Features.Courses;

public class CourseService : ICourseService
{
    private readonly IAppDbContext _db;

    public CourseService(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CourseDto>> GetAllAsync(bool onlyActive, CancellationToken ct = default)
    {
        var query = _db.Courses.AsNoTracking();
        if (onlyActive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Title)
            .Select(c => new CourseDto(
                c.Id, c.Title, c.Description, c.Price, c.DurationMonths, c.Level, c.IsActive,
                c.Groups.Count))
            .ToListAsync(ct);
    }

    public async Task<CourseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var course = await _db.Courses.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseDto(
                c.Id, c.Title, c.Description, c.Price, c.DurationMonths, c.Level, c.IsActive,
                c.Groups.Count))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Kurs", id);

        return course;
    }

    public async Task<CourseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct = default)
    {
        var course = new Course
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Price = request.Price,
            DurationMonths = request.DurationMonths,
            Level = request.Level,
            IsActive = true
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync(ct);

        return ToDto(course, 0);
    }

    public async Task<CourseDto> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken ct = default)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Kurs", id);

        course.Title = request.Title.Trim();
        course.Description = request.Description;
        course.Price = request.Price;
        course.DurationMonths = request.DurationMonths;
        course.Level = request.Level;
        course.IsActive = request.IsActive;
        course.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var groupsCount = await _db.Groups.CountAsync(g => g.CourseId == id, ct);
        return ToDto(course, groupsCount);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var course = await _db.Courses
            .Include(c => c.Groups)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Kurs", id);

        if (course.Groups.Count > 0)
            throw new ConflictException("Kursga bog'langan guruhlar mavjud. Avval guruhlarni o'chiring yoki kursni nofaol qiling.");

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync(ct);
    }

    private static CourseDto ToDto(Course c, int groupsCount) => new(
        c.Id, c.Title, c.Description, c.Price, c.DurationMonths, c.Level, c.IsActive, groupsCount);
}