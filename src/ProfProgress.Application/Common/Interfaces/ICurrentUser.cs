using ProfProgress.Domain.Enums;

namespace ProfProgress.Application.Common.Interfaces;

/// <summary>
/// Joriy autentifikatsiyalangan foydalanuvchi haqida ma'lumot
/// (HTTP kontekstdan olinadi, Api qatlamida implement qilinadi).
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}