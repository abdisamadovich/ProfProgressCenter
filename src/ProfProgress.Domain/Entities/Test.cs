using ProfProgress.Domain.Common;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Domain.Entities;

/// <summary>
/// Test (imtihon). Savollar Telegram orqali yuborilgan PDF/Word faylda bo'ladi,
/// talaba faqat variantlarni (A/B/C/D) belgilaydi. Avtomatik baholanadi.
/// </summary>
public class Test : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Telegram'dagi fayl identifikatori — botda qayta yuborish uchun.</summary>
    public string? TelegramFileId { get; set; }
    public string? FileName { get; set; }

    public int QuestionCount { get; set; }

    /// <summary>Variantlar soni: 4 = A–D, 5 = A–E.</summary>
    public int OptionCount { get; set; } = 4;

    /// <summary>To'g'ri javoblar kaliti, masalan "ABCDA..." (uzunligi = QuestionCount).</summary>
    public string AnswerKey { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    /// <summary>Hamma uchun belgilangan umumiy boshlanish vaqti (UTC).</summary>
    public DateTime StartsAt { get; set; }

    /// <summary>Test tegishli guruh. Null bo'lsa — barcha bog'langan talabalar.</summary>
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public TestStatus Status { get; set; } = TestStatus.Draft;

    // Navigatsiya
    public ICollection<TestBlock> Blocks { get; set; } = new List<TestBlock>();
    public ICollection<TestAttempt> Attempts { get; set; } = new List<TestAttempt>();

    public DateTime EndsAt => StartsAt.AddMinutes(DurationMinutes);
}
