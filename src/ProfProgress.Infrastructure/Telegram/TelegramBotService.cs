using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Application.Common.Settings;
using ProfProgress.Application.Features.Telegram;
using ProfProgress.Application.Features.Tests;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ProfProgress.Infrastructure.Telegram;

/// <summary>
/// Telegram bot: telefon orqali bog'lanish, admin test yaratish sehrgari,
/// talaba javoblarini qabul qilish va baholash, jadval bo'yicha test yuborish.
/// </summary>
public class TelegramBotService : BackgroundService
{
    private readonly TelegramSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConversationStore _store;
    private readonly ILogger<TelegramBotService> _logger;

    private TelegramBotClient? _bot;

    public TelegramBotService(
        IOptions<TelegramSettings> settings,
        IServiceScopeFactory scopeFactory,
        ConversationStore store,
        ILogger<TelegramBotService> logger)
    {
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.BotToken))
        {
            _logger.LogWarning("Telegram bot o'chirilgan: BotToken kiritilmagan (appsettings → Telegram:BotToken).");
            return;
        }

        _bot = new TelegramBotClient(_settings.BotToken);
        _logger.LogInformation("Telegram bot ishga tushdi.");

        await Task.WhenAll(PollingLoopAsync(stoppingToken), SchedulerLoopAsync(stoppingToken));
    }

    // ======================= Polling =======================
    private async Task PollingLoopAsync(CancellationToken ct)
    {
        int offset = 0;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var updates = await _bot!.GetUpdates(offset, limit: 100, timeout: 30, cancellationToken: ct);
                foreach (var update in updates)
                {
                    offset = update.Id + 1;
                    try { await HandleUpdateAsync(update, ct); }
                    catch (Exception ex) { _logger.LogError(ex, "Update'ni qayta ishlashda xato"); }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUpdates xatosi");
                await Task.Delay(3000, ct);
            }
        }
    }

    private async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        if (update.Message is not { } message) return;
        var chatId = message.Chat.Id;

        // Telefon raqami (contact) — bog'lash
        if (message.Contact is { } contact)
        {
            await HandleContactAsync(chatId, contact.PhoneNumber, ct);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var accounts = scope.ServiceProvider.GetRequiredService<ITelegramAccountService>();
        var linkedUser = await accounts.GetByChatIdAsync(chatId, ct);

        // Bog'lanmagan foydalanuvchi — avval telefon so'raymiz
        if (linkedUser is null)
        {
            await RequestContactAsync(chatId, ct);
            return;
        }

        var text = message.Text?.Trim() ?? string.Empty;

        // Buyruqlar
        if (text.StartsWith('/'))
        {
            await HandleCommandAsync(scope, chatId, text, linkedUser, ct);
            return;
        }

        // Admin sehrgar jarayonida bo'lsa
        var state = _store.Get(chatId);
        if (state.Step != ConversationStep.Idle && linkedUser.IsStaff)
        {
            await HandleWizardAsync(scope, chatId, message, state, ct);
            return;
        }

        // Talaba javob yuborishi
        if (linkedUser.StudentId is Guid studentId)
        {
            await HandleAnswerSubmissionAsync(scope, chatId, studentId, text, ct);
            return;
        }

        await SendAsync(chatId, "Buyruq tushunarsiz. /help ni bosing.", ct);
    }

    // ======================= Bog'lanish =======================
    private async Task RequestContactAsync(long chatId, CancellationToken ct)
    {
        var kb = new ReplyKeyboardMarkup(new[]
        {
            new[] { KeyboardButton.WithRequestContact("📱 Telefon raqamni yuborish") },
        })
        { ResizeKeyboard = true, OneTimeKeyboard = true };

        await _bot!.SendMessage(chatId,
            "Assalomu alaykum! 👋\nProfessionalProgressCenter botiga xush kelibsiz.\n\n" +
            "Davom etish uchun telefon raqamingizni yuboring (saytda ro'yxatdan o'tgan raqam).",
            replyMarkup: kb, cancellationToken: ct);
    }

    private async Task HandleContactAsync(long chatId, string phone, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var accounts = scope.ServiceProvider.GetRequiredService<ITelegramAccountService>();
        var linked = await accounts.LinkByPhoneAsync(phone, chatId, ct);

        if (linked is null)
        {
            await _bot!.SendMessage(chatId,
                "❌ Bu raqam saytda topilmadi. Avval saytda ro'yxatdan o'ting yoki administrator bilan bog'laning.",
                replyMarkup: new ReplyKeyboardRemove(), cancellationToken: ct);
            return;
        }

        var role = linked.IsStaff ? "Administrator/o'qituvchi" : "O'quvchi";
        var hint = linked.IsStaff
            ? "Yangi test yaratish uchun /newtest buyrug'ini yuboring."
            : "Test boshlanganda men sizga faylni yuboraman. Javoblaringizni '1a2b3c...' ko'rinishida yuborasiz.";

        await _bot!.SendMessage(chatId,
            $"✅ Muvaffaqiyatli bog'landingiz!\n\n👤 {linked.FullName}\n🔖 {role}\n\n{hint}",
            replyMarkup: new ReplyKeyboardRemove(), cancellationToken: ct);
    }

    // ======================= Buyruqlar =======================
    private async Task HandleCommandAsync(IServiceScope scope, long chatId, string text, LinkedUserDto user, CancellationToken ct)
    {
        var cmd = text.Split(' ')[0].ToLowerInvariant();
        switch (cmd)
        {
            case "/start":
                await SendAsync(chatId, $"Xush kelibsiz, {user.FullName}!\n" +
                    (user.IsStaff ? "Test yaratish: /newtest" : "Testlar boshlanganda xabar beraman."), ct);
                break;

            case "/help":
                await SendAsync(chatId, user.IsStaff
                    ? "/newtest — yangi test yaratish\n/cancel — jarayonni bekor qilish"
                    : "Test boshlanganda fayl keladi. Javoblarni '1a2b3c...' ko'rinishida yuboring.", ct);
                break;

            case "/newtest":
                if (!user.IsStaff)
                {
                    await SendAsync(chatId, "Bu buyruq faqat administrator/o'qituvchilar uchun.", ct);
                    return;
                }
                await StartWizardAsync(chatId, ct);
                break;

            case "/cancel":
                _store.Reset(chatId);
                await SendAsync(chatId, "Bekor qilindi.", ct);
                break;

            default:
                await SendAsync(chatId, "Noma'lum buyruq. /help", ct);
                break;
        }
    }

    // ======================= Admin sehrgari =======================
    private async Task StartWizardAsync(long chatId, CancellationToken ct)
    {
        var state = _store.Get(chatId);
        state.Step = ConversationStep.AwaitingFile;
        state.Draft = new TestDraft();
        await SendAsync(chatId, "🆕 <b>Yangi test</b>\n\n1-qadam: Test savollari joylashgan faylni (PDF/Word) yuboring.\n(Faylsiz davom etish uchun \"yo'q\" deb yozing.)", ct);
    }

    private async Task HandleWizardAsync(IServiceScope scope, long chatId, Message message, ConversationState state, CancellationToken ct)
    {
        var text = message.Text?.Trim() ?? string.Empty;
        var d = state.Draft;
        switch (state.Step)
        {
            case ConversationStep.AwaitingFile:
                if (message.Document is { } doc)
                {
                    d.FileId = doc.FileId;
                    d.FileName = doc.FileName;
                    state.Step = ConversationStep.AwaitingTitle;
                    await SendAsync(chatId, "📎 Fayl qabul qilindi.\n2-qadam: Test nomini kiriting.", ct);
                    return;
                }
                // Matn kelsa — "yo'q" bo'lsa faylsiz davom; aks holda fayl kutyapmiz.
                if (!string.Equals(text, "yo'q", StringComparison.OrdinalIgnoreCase) && text != "yoq")
                {
                    await SendAsync(chatId, "Iltimos, faylni (PDF/Word) yuboring yoki \"yo'q\" deb yozing.", ct);
                    return;
                }
                state.Step = ConversationStep.AwaitingTitle;
                await SendAsync(chatId, "2-qadam: Test nomini kiriting.", ct);
                break;

            case ConversationStep.AwaitingTitle:
                d.Title = text;
                state.Step = ConversationStep.AwaitingQuestionCount;
                await SendAsync(chatId, "3-qadam: Savollar sonini kiriting (masalan: 30).", ct);
                break;

            case ConversationStep.AwaitingQuestionCount:
                if (!int.TryParse(text, out var qc) || qc < 1 || qc > 200)
                {
                    await SendAsync(chatId, "Noto'g'ri. 1–200 oralig'ida son kiriting.", ct);
                    return;
                }
                d.QuestionCount = qc;
                state.Step = ConversationStep.AwaitingAnswerKey;
                await SendAsync(chatId, $"4-qadam: To'g'ri javoblar kalitini kiriting ({qc} ta harf).\nMasalan: <code>ABCDABCD...</code>", ct);
                break;

            case ConversationStep.AwaitingAnswerKey:
                var key = GradingHelper.Normalize(text, 8);
                if (key.Length != d.QuestionCount)
                {
                    await SendAsync(chatId, $"Kalit uzunligi {key.Length} ta bo'ldi, kerak: {d.QuestionCount} ta. Qayta kiriting.", ct);
                    return;
                }
                d.AnswerKey = key;
                state.Step = ConversationStep.AwaitingBlocks;
                await SendAsync(chatId,
                    "5-qadam: Bloklar va ballarni kiriting.\nMasalan: <code>1-10:1.1, 11-20:2.1, 21-30:3.1</code>\n" +
                    "Yoki barcha savollar 1 balldan bo'lsa \"yo'q\" deb yozing.", ct);
                break;

            case ConversationStep.AwaitingBlocks:
                if (string.Equals(text, "yo'q", StringComparison.OrdinalIgnoreCase) || text == "yoq")
                {
                    d.Blocks = new();
                }
                else if (!TryParseBlocks(text, d.QuestionCount, out var blocks, out var err))
                {
                    await SendAsync(chatId, $"Xato: {err}\nQayta kiriting yoki \"yo'q\".", ct);
                    return;
                }
                else
                {
                    d.Blocks = blocks;
                }
                await PromptGroupAsync(scope, chatId, state, ct);
                break;

            case ConversationStep.AwaitingGroup:
                if (!int.TryParse(text, out var gi) || gi < 0 || gi > d.GroupOptions.Count)
                {
                    await SendAsync(chatId, "Ro'yxatdagi raqamni kiriting (0 = barcha o'quvchilar).", ct);
                    return;
                }
                if (gi == 0) { d.GroupId = null; d.GroupName = "Barcha o'quvchilar"; }
                else { var g = d.GroupOptions[gi - 1]; d.GroupId = g.Id; d.GroupName = g.Name; }
                state.Step = ConversationStep.AwaitingStart;
                await SendAsync(chatId, $"7-qadam: Boshlanish vaqtini kiriting (mahalliy vaqt).\nMasalan: <code>{DateTime.UtcNow.AddHours(_settings.TimezoneOffsetHours).AddMinutes(10):yyyy-MM-dd HH:mm}</code>", ct);
                break;

            case ConversationStep.AwaitingStart:
                if (!TryParseLocalTime(text, out var startUtc))
                {
                    await SendAsync(chatId, "Noto'g'ri format. Masalan: 2026-06-20 14:30", ct);
                    return;
                }
                d.StartsAtUtc = startUtc;
                state.Step = ConversationStep.AwaitingDuration;
                await SendAsync(chatId, "8-qadam: Test davomiyligini kiriting (daqiqada, masalan 60).", ct);
                break;

            case ConversationStep.AwaitingDuration:
                if (!int.TryParse(text, out var dur) || dur < 1 || dur > 600)
                {
                    await SendAsync(chatId, "1–600 oralig'ida son kiriting.", ct);
                    return;
                }
                d.DurationMinutes = dur;
                state.Step = ConversationStep.AwaitingConfirm;
                await SendAsync(chatId, BuildSummary(d) + "\n\nTasdiqlaysizmi? <b>ha</b> / <b>yo'q</b>", ct);
                break;

            case ConversationStep.AwaitingConfirm:
                if (string.Equals(text, "ha", StringComparison.OrdinalIgnoreCase))
                {
                    await CreateTestAsync(scope, chatId, state, ct);
                }
                else
                {
                    _store.Reset(chatId);
                    await SendAsync(chatId, "Bekor qilindi.", ct);
                }
                break;
        }
    }

    private async Task PromptGroupAsync(IServiceScope scope, long chatId, ConversationState state, CancellationToken ct)
    {
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var groups = db.Groups.Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .Select(g => new { g.Id, g.Name })
            .ToList();

        state.Draft.GroupOptions = groups.Select(g => (g.Id, g.Name)).ToList();
        state.Step = ConversationStep.AwaitingGroup;

        var lines = new List<string> { "6-qadam: Qaysi guruh topshiradi? Raqamni kiriting:", "", "0 — Barcha o'quvchilar" };
        for (int i = 0; i < groups.Count; i++)
            lines.Add($"{i + 1} — {groups[i].Name}");

        await SendAsync(chatId, string.Join('\n', lines), ct);
    }

    private async Task CreateTestAsync(IServiceScope scope, long chatId, ConversationState state, CancellationToken ct)
    {
        var d = state.Draft;
        var accounts = scope.ServiceProvider.GetRequiredService<ITelegramAccountService>();
        var user = await accounts.GetByChatIdAsync(chatId, ct);
        if (user is null) { _store.Reset(chatId); return; }

        var tests = scope.ServiceProvider.GetRequiredService<ITestService>();
        await tests.CreateAsync(new CreateTestCommand(
            d.Title, null, d.FileId, d.FileName, d.QuestionCount, d.OptionCount,
            d.AnswerKey, d.DurationMinutes, d.StartsAtUtc, d.GroupId, user.UserId, d.Blocks), ct);

        _store.Reset(chatId);
        var localStart = d.StartsAtUtc.AddHours(_settings.TimezoneOffsetHours);
        await SendAsync(chatId,
            $"✅ Test yaratildi va rejalashtirildi!\n\n📋 {d.Title}\n🕒 Boshlanish: {localStart:yyyy-MM-dd HH:mm}\n" +
            $"⏱ Davomiyligi: {d.DurationMinutes} daqiqa\n👥 {d.GroupName}\n\n" +
            "Belgilangan vaqtda test o'quvchilarga avtomatik yuboriladi.", ct);
    }

    // ======================= Talaba javoblari =======================
    private async Task HandleAnswerSubmissionAsync(IServiceScope scope, long chatId, Guid studentId, string text, CancellationToken ct)
    {
        var tests = scope.ServiceProvider.GetRequiredService<ITestService>();
        var active = await tests.GetActiveTestForStudentAsync(studentId, ct);
        if (active is null)
        {
            await SendAsync(chatId, "Hozir sizda faol test yo'q. Test boshlanganda men xabar beraman.", ct);
            return;
        }

        try
        {
            var result = await tests.SubmitAsync(active.Id, studentId, text, ct);
            await SendAsync(chatId,
                $"✅ Javoblaringiz qabul qilindi!\n\n📋 {active.Title}\n" +
                $"✔️ To'g'ri: {result.CorrectCount} / {result.QuestionCount}\n" +
                $"🏆 Ball: {result.TotalScore} / {result.MaxScore}\n\n" +
                "Batafsil natijani saytdagi shaxsiy kabinetingizdan ko'rishingiz mumkin.", ct);
        }
        catch (Exception ex)
        {
            await SendAsync(chatId, $"⚠️ {ex.Message}", ct);
        }
    }

    // ======================= Scheduler =======================
    private async Task SchedulerLoopAsync(CancellationToken ct)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(20));
        while (await timer.WaitForNextTickAsync(ct))
        {
            try { await SchedulerTickAsync(ct); }
            catch (Exception ex) { _logger.LogError(ex, "Scheduler xatosi"); }
        }
    }

    private async Task SchedulerTickAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var tests = scope.ServiceProvider.GetRequiredService<ITestService>();
        var now = DateTime.UtcNow;

        // Boshlanadigan testlar — fayl yuborish
        var due = await tests.StartDueTestsAsync(now, ct);
        foreach (var t in due)
        {
            var recipients = await tests.GetRecipientsAsync(t.Id, ct);
            foreach (var r in recipients)
            {
                try { await SendTestToStudentAsync(r.ChatId, t, ct); }
                catch (Exception ex) { _logger.LogWarning(ex, "Testni yuborishda xato: {Chat}", r.ChatId); }
            }
            _logger.LogInformation("Test '{Title}' {Count} ta o'quvchiga yuborildi.", t.Title, recipients.Count);
        }

        // Yakunlanadigan testlar
        await tests.FinishDueTestsAsync(now, ct);
    }

    private async Task SendTestToStudentAsync(long chatId, DueTestDto test, CancellationToken ct)
    {
        var caption =
            $"📢 <b>Test boshlandi!</b>\n\n📋 {test.Title}\n❓ Savollar: {test.QuestionCount} ta\n⏱ Vaqt: {test.DurationMinutes} daqiqa\n\n" +
            "Javoblaringizni bitta xabarda yuboring.\nMasalan: <code>1a2b3c4d</code> yoki <code>abcd...</code>";

        if (!string.IsNullOrEmpty(test.TelegramFileId))
        {
            await _bot!.SendDocument(chatId, InputFile.FromFileId(test.TelegramFileId!),
                caption: caption, parseMode: ParseMode.Html, cancellationToken: ct);
        }
        else
        {
            await _bot!.SendMessage(chatId, caption, parseMode: ParseMode.Html, cancellationToken: ct);
        }
    }

    // ======================= Yordamchilar =======================
    private Task SendAsync(long chatId, string text, CancellationToken ct) =>
        _bot!.SendMessage(chatId, text, parseMode: ParseMode.Html, cancellationToken: ct);

    private static string BuildSummary(TestDraft d)
    {
        var blocks = d.Blocks.Count == 0
            ? "Har savol 1 ball"
            : string.Join(", ", d.Blocks.Select(b => $"{b.FromQuestion}-{b.ToQuestion}:{b.PointsPerQuestion}"));
        return $"<b>Tekshiring:</b>\n📋 {d.Title}\n❓ Savollar: {d.QuestionCount}\n🔑 Kalit: {d.AnswerKey}\n" +
               $"🧩 Bloklar: {blocks}\n👥 {d.GroupName}\n⏱ {d.DurationMinutes} daqiqa";
    }

    private bool TryParseLocalTime(string text, out DateTime utc)
    {
        utc = default;
        if (!DateTime.TryParse(text, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var local))
            return false;
        utc = DateTime.SpecifyKind(local.AddHours(-_settings.TimezoneOffsetHours), DateTimeKind.Utc);
        return true;
    }

    private static bool TryParseBlocks(string text, int questionCount, out List<TestBlockInput> blocks, out string error)
    {
        blocks = new();
        error = string.Empty;
        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            var rangeAndPoints = part.Split(':', StringSplitOptions.TrimEntries);
            if (rangeAndPoints.Length != 2) { error = $"'{part}' noto'g'ri (format: 1-10:1.1)"; return false; }

            var range = rangeAndPoints[0].Split('-', StringSplitOptions.TrimEntries);
            if (range.Length != 2 || !int.TryParse(range[0], out var from) || !int.TryParse(range[1], out var to))
            { error = $"'{part}' oralig'i noto'g'ri"; return false; }

            if (!decimal.TryParse(rangeAndPoints[1], System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var points))
            { error = $"'{part}' bali noto'g'ri"; return false; }

            if (from < 1 || to > questionCount || from > to)
            { error = $"'{part}' oralig'i 1–{questionCount} ichida bo'lsin"; return false; }

            blocks.Add(new TestBlockInput(null, from, to, points));
        }
        return blocks.Count > 0;
    }
}
