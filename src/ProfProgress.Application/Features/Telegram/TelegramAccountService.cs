using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Application.Features.Telegram;

public class TelegramAccountService : ITelegramAccountService
{
    private readonly IAppDbContext _db;

    public TelegramAccountService(IAppDbContext db) => _db = db;

    public async Task<LinkedUserDto?> LinkByPhoneAsync(string phone, long chatId, CancellationToken ct = default)
    {
        var suffix = DigitsSuffix(phone);
        if (suffix.Length < 7) return null;

        // Telefon raqamining oxirgi 9 raqami bo'yicha solishtiramiz.
        var candidates = await _db.Users
            .Include(u => u.Student)
            .Where(u => u.IsActive)
            .ToListAsync(ct);

        var user = candidates.FirstOrDefault(u => DigitsSuffix(u.Phone) == suffix);
        if (user is null) return null;

        // Agar shu chat boshqa userga bog'langan bo'lsa — uzamiz.
        var others = candidates.Where(u => u.TelegramChatId == chatId && u.Id != user.Id);
        foreach (var o in others) o.TelegramChatId = null;

        user.TelegramChatId = chatId;
        await _db.SaveChangesAsync(ct);

        return ToDto(user);
    }

    public async Task<LinkedUserDto?> GetByChatIdAsync(long chatId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);

        return user is null ? null : ToDto(user);
    }

    private static LinkedUserDto ToDto(User user) => new(
        user.Id,
        user.Student?.Id,
        user.Role,
        user.FullName,
        user.Role is UserRole.Admin or UserRole.SuperAdmin or UserRole.Teacher);

    /// <summary>Raqamdan faqat raqamlarni olib, oxirgi 9 tasini qaytaradi (O'zbekiston raqamlari).</summary>
    private static string DigitsSuffix(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length <= 9 ? digits : digits[^9..];
    }
}
