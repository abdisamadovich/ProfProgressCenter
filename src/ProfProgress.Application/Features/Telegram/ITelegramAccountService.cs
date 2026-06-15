namespace ProfProgress.Application.Features.Telegram;

public interface ITelegramAccountService
{
    /// <summary>Telefon raqami orqali Telegram chatni foydalanuvchiga bog'laydi.</summary>
    Task<LinkedUserDto?> LinkByPhoneAsync(string phone, long chatId, CancellationToken ct = default);

    /// <summary>Chat Id bo'yicha bog'langan foydalanuvchini qaytaradi (yo'q bo'lsa null).</summary>
    Task<LinkedUserDto?> GetByChatIdAsync(long chatId, CancellationToken ct = default);
}
