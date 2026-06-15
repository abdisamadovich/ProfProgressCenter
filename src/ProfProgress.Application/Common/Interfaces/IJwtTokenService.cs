using ProfProgress.Domain.Entities;

namespace ProfProgress.Application.Common.Interfaces;

public record TokenResult(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);

public interface IJwtTokenService
{
    TokenResult CreateTokens(User user);

    string GenerateRefreshToken();
}