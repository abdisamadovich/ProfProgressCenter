namespace ProfProgress.Application.Common.Settings;

public class TelegramSettings
{
    public const string SectionName = "Telegram";

    /// <summary>@BotFather'dan olingan bot tokeni. Bo'sh bo'lsa bot ishga tushmaydi.</summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>Mahalliy vaqt mintaqasi (UTC'dan farqi, soatda). O'zbekiston = 5.</summary>
    public int TimezoneOffsetHours { get; set; } = 5;
}
