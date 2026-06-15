using ProfProgress.Domain.Common;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// Tizim foydalanuvchisi (autentifikatsiya uchun). Har bir foydalanuvchi
/// roliga qarab Student yoki Teacher profiliga ega bo'lishi mumkin.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public bool IsActive { get; set; } = true;

    // Refresh token (oddiy yondashuv — bitta faol token)
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Telegram bog'lanishi (telefon raqami orqali ulanadi)
    public long? TelegramChatId { get; set; }

    // Navigatsiya
    public Student? Student { get; set; }

    public Teacher? Teacher { get; set; }
}