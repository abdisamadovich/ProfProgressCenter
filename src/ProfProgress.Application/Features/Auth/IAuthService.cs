namespace ProfProgress.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default);

    Task<UserDto> GetCurrentAsync(Guid userId, CancellationToken ct = default);
}