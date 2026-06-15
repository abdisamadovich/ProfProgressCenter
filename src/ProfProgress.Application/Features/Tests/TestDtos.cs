namespace ProfProgress.Application.Features.Tests;

/// <summary>Bot ushlab turgan sehrgar (wizard) yig'gan ma'lumotlar asosida test yaratish.</summary>
public record CreateTestCommand(
    string Title,
    string? Description,
    string? TelegramFileId,
    string? FileName,
    int QuestionCount,
    int OptionCount,
    string AnswerKey,
    int DurationMinutes,
    DateTime StartsAt,
    Guid? GroupId,
    Guid CreatedByUserId,
    List<TestBlockInput> Blocks);

public record TestBlockInput(string? Name, int FromQuestion, int ToQuestion, decimal PointsPerQuestion);

/// <summary>Admin uchun test ro'yxati elementi (statistika bilan).</summary>
public record TestSummaryDto(
    Guid Id,
    string Title,
    int QuestionCount,
    DateTime StartsAt,
    int DurationMinutes,
    string Status,
    string? GroupName,
    int SubmittedCount,
    decimal MaxScore);

/// <summary>Bitta urinish natijasi (admin natijalar jadvali uchun).</summary>
public record AttemptResultDto(
    Guid Id,
    string StudentName,
    DateTime? SubmittedAt,
    int CorrectCount,
    int QuestionCount,
    decimal TotalScore,
    decimal MaxScore,
    string Status);

/// <summary>Test bo'yicha to'liq natijalar + tahlil.</summary>
public record TestResultsDto(
    TestSummaryDto Test,
    IReadOnlyList<AttemptResultDto> Attempts,
    IReadOnlyList<QuestionStatDto> QuestionStats);

/// <summary>Savol bo'yicha tahlil — nechta talaba to'g'ri topgan.</summary>
public record QuestionStatDto(int QuestionNumber, char CorrectOption, int CorrectCount, int TotalAnswered);

/// <summary>Talaba o'z natijasini ko'rishi uchun.</summary>
public record StudentTestResultDto(
    Guid TestId,
    string Title,
    DateTime StartsAt,
    string? GroupName,
    int CorrectCount,
    int QuestionCount,
    decimal TotalScore,
    decimal MaxScore,
    string Status,
    string? Answers,
    string AnswerKey);

/// <summary>Baholash natijasi (bot talabaga javob qaytarish uchun).</summary>
public record SubmitResult(
    int CorrectCount,
    int QuestionCount,
    decimal TotalScore,
    decimal MaxScore,
    IReadOnlyList<int> WrongQuestions);

/// <summary>Test yuboriladigan talaba (Telegramga bog'langan).</summary>
public record RecipientDto(Guid StudentId, long ChatId, string FullName);

/// <summary>Bot uchun faol test ma'lumoti.</summary>
public record ActiveTestDto(
    Guid Id,
    string Title,
    string? TelegramFileId,
    int QuestionCount,
    DateTime EndsAt);

/// <summary>Boshlash/yakunlash uchun navbatdagi test (bot scheduler uchun).</summary>
public record DueTestDto(Guid Id, string Title, string? TelegramFileId, int QuestionCount, int DurationMinutes);
