using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using ProfProgress.Domain.Enums;
using System.Linq.Expressions;

namespace ProfProgress.Application.Features.People;

public class TeacherService : ITeacherService
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public TeacherService(IAppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<IReadOnlyList<TeacherDto>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Teachers.AsNoTracking()
            .OrderBy(t => t.User.FullName)
            .Select(Projection)
            .ToListAsync(ct);

    public async Task<TeacherDto> CreateAsync(CreateTeacherRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("Bu email allaqachon ro'yxatdan o'tgan.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            Role = UserRole.Teacher,
            IsActive = true,
            Teacher = new Teacher
            {
                Specialization = request.Specialization,
                Bio = request.Bio
            }
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return await _db.Teachers.AsNoTracking()
            .Where(t => t.UserId == user.Id)
            .Select(Projection)
            .FirstAsync(ct);
    }

    private static readonly Expression<Func<Teacher, TeacherDto>> Projection = t => new TeacherDto(
        t.Id,
        t.UserId,
        t.User.FullName,
        t.User.Email,
        t.User.Phone,
        t.Specialization,
        t.Bio,
        t.User.IsActive,
        t.Groups.Count);
}