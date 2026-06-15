using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public const string DefaultAdminEmail = "admin@profprogress.uz";
    public const string DefaultAdminPassword = "Admin123!";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        await SeedAdminAsync(db, hasher, ct);
        await SeedCoursesAsync(db, ct);
    }

    private static async Task SeedAdminAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin, ct))
            return;

        db.Users.Add(new User
        {
            FullName = "Bosh administrator",
            Email = DefaultAdminEmail,
            Phone = "+998900000000",
            PasswordHash = hasher.Hash(DefaultAdminPassword),
            Role = UserRole.SuperAdmin,
            IsActive = true
        });

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedCoursesAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Courses.AnyAsync(ct))
            return;

        db.Courses.AddRange(
            new Course { Title = "Frontend dasturlash (HTML, CSS, JS)", Description = "Web sahifalar yaratishni 0 dan o'rganamiz.", Price = 600000, DurationMonths = 4, Level = CourseLevel.Beginner },
            new Course { Title = "Angular bilan SPA ishlab chiqish", Description = "Zamonaviy frontend framework.", Price = 900000, DurationMonths = 3, Level = CourseLevel.Intermediate },
            new Course { Title = "Ingliz tili — General English", Description = "Boshlang'ich va o'rta daraja.", Price = 500000, DurationMonths = 6, Level = CourseLevel.Beginner }
        );

        await db.SaveChangesAsync(ct);
    }
}