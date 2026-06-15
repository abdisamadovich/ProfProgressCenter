using ProfProgress.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProfProgress.Application.Features.Auth;

public record RegisterRequest(
    [Required, MaxLength(150)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MaxLength(20)] string Phone,
    [Required, MinLength(6)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RefreshTokenRequest(
    [Required] string RefreshToken);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    UserRole Role);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserDto User);