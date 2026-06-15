using System.Collections.Concurrent;
using ProfProgress.Application.Features.Tests;

namespace ProfProgress.Infrastructure.Telegram;

public enum ConversationStep
{
    Idle = 0,
    AwaitingFile,
    AwaitingTitle,
    AwaitingQuestionCount,
    AwaitingAnswerKey,
    AwaitingBlocks,
    AwaitingGroup,
    AwaitingStart,
    AwaitingDuration,
    AwaitingConfirm,
}

/// <summary>Admin test yaratish jarayonida yig'ilayotgan ma'lumotlar.</summary>
public class TestDraft
{
    public string? FileId { get; set; }
    public string? FileName { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int OptionCount { get; set; } = 4;
    public string AnswerKey { get; set; } = string.Empty;
    public List<TestBlockInput> Blocks { get; set; } = new();
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public int DurationMinutes { get; set; }

    /// <summary>Guruh tanlash uchun raqamlangan ro'yxat (index → group).</summary>
    public List<(Guid Id, string Name)> GroupOptions { get; set; } = new();
}

public class ConversationState
{
    public ConversationStep Step { get; set; } = ConversationStep.Idle;
    public TestDraft Draft { get; set; } = new();
}

/// <summary>Chat bo'yicha suhbat holatini xotirada saqlaydigan singleton.</summary>
public class ConversationStore
{
    private readonly ConcurrentDictionary<long, ConversationState> _states = new();

    public ConversationState Get(long chatId) => _states.GetOrAdd(chatId, _ => new ConversationState());

    public void Reset(long chatId) => _states.TryRemove(chatId, out _);

    public bool IsBusy(long chatId) =>
        _states.TryGetValue(chatId, out var s) && s.Step != ConversationStep.Idle;
}
