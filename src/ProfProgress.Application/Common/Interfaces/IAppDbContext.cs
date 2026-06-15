using Microsoft.EntityFrameworkCore;
using ProfProgress.Domain.Entities;

namespace ProfProgress.Application.Common.Interfaces;

/// <summary>
/// DbContext abstraksiyasi. Application qatlami EF Core'ning konkret
/// implementatsiyasiga emas, shu interfeysga bog'lanadi.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Student> Students { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Course> Courses { get; }
    DbSet<Group> Groups { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<Grade> Grades { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}