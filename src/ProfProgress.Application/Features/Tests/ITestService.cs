namespace ProfProgress.Application.Features.Tests;

public interface ITestService
{
    // ===== Yaratish (bot orqali) =====
    Task<Guid> CreateAsync(CreateTestCommand command, CancellationToken ct = default);

    // ===== Sayt — admin =====
    Task<IReadOnlyList<TestSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<TestResultsDto> GetResultsAsync(Guid testId, CancellationToken ct = default);
    Task DeleteAsync(Guid testId, CancellationToken ct = default);

    // ===== Sayt — talaba =====
    Task<IReadOnlyList<StudentTestResultDto>> GetMyResultsAsync(Guid userId, CancellationToken ct = default);

    // ===== Bot — topshirish =====
    Task<ActiveTestDto?> GetActiveTestForStudentAsync(Guid studentId, CancellationToken ct = default);
    Task<SubmitResult> SubmitAsync(Guid testId, Guid studentId, string rawAnswers, CancellationToken ct = default);

    // ===== Bot — scheduler =====
    Task<IReadOnlyList<DueTestDto>> StartDueTestsAsync(DateTime nowUtc, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> FinishDueTestsAsync(DateTime nowUtc, CancellationToken ct = default);
    Task<IReadOnlyList<RecipientDto>> GetRecipientsAsync(Guid testId, CancellationToken ct = default);
}
