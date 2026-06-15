using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthService(IAppDbContext db, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.Email == email, ct);
        if (exists)
            throw new ConflictException("Bu email allaqachon ro'yxatdan o'tgan.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            Role = UserRole.Student,
            IsActive = true,
            Student = new Student()
        };

        _db.Users.Add(user);
        var tokens = await IssueTokensAsync(user, ct);

        return BuildResponse(user, tokens);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAppException("Email yoki parol noto'g'ri.");

        if (!user.IsActive)
            throw new ForbiddenException("Hisob bloklangan. Administrator bilan bog'laning.");

        var tokens = await IssueTokensAsync(user, ct);
        return BuildResponse(user, tokens);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, ct);

        if (user is null || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAppException("Refresh token yaroqsiz yoki muddati tugagan.");

        var tokens = await IssueTokensAsync(user, ct);
        return BuildResponse(user, tokens);
    }

    public async Task<UserDto> GetCurrentAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("Foydalanuvchi", userId);

        return ToDto(user);
    }

    // --- yordamchi metodlar ---

    private async Task<TokenResult> IssueTokensAsync(User user, CancellationToken ct)
    {
        var tokens = _jwt.CreateTokens(user);
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt;
        await _db.SaveChangesAsync(ct);
        return tokens;
    }

    private static AuthResponse BuildResponse(User user, TokenResult tokens) => new(
        tokens.AccessToken,
        tokens.AccessTokenExpiresAt,
        tokens.RefreshToken,
        tokens.RefreshTokenExpiresAt,
        ToDto(user));

    private static UserDto ToDto(User u) =>
        new(u.Id, u.FullName, u.Email, u.Phone, u.Role);
}