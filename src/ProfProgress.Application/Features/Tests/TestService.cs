using Microsoft.EntityFrameworkCore;
using ProfProgress.Application.Common.Exceptions;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Entities;
using ProfProgress.Domain.Enums;

namespace ProfProgress.Application.Features.Tests;

public class TestService : ITestService
{
    private readonly IAppDbContext _db;

    public TestService(IAppDbContext db) => _db = db;

    // ===================== Yaratish =====================
    public async Task<Guid> CreateAsync(CreateTestCommand command, CancellationToken ct = default)
    {
        var test = new Test
        {
            Title = command.Title.Trim(),
            Description = command.Description,
            TelegramFileId = command.TelegramFileId,
            FileName = command.FileName,
            QuestionCount = command.QuestionCount,
            OptionCount = command.OptionCount,
            AnswerKey = command.AnswerKey.ToUpperInvariant(),
            DurationMinutes = command.DurationMinutes,
            StartsAt = command.StartsAt,
            GroupId = command.GroupId,
            CreatedByUserId = command.CreatedByUserId,
            Status = TestStatus.Scheduled,
            Blocks = command.Blocks.Select(b => new TestBlock
            {
                Name = b.Name,
                FromQuestion = b.FromQuestion,
                ToQuestion = b.ToQuestion,
                PointsPerQuestion = b.PointsPerQuestion,
            }).ToList(),
        };

        _db.Tests.Add(test);
        await _db.SaveChangesAsync(ct);
        return test.Id;
    }

    // ===================== Sayt — admin =====================
    public async Task<IReadOnlyList<TestSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tests = await _db.Tests
            .AsNoTracking()
            .Include(t => t.Blocks)
            .Include(t => t.Group)
            .OrderByDescending(t => t.StartsAt)
            .ToListAsync(ct);

        var counts = await _db.TestAttempts
            .Where(a => a.Status == AttemptStatus.Submitted)
            .GroupBy(a => a.TestId)
            .Select(g => new { TestId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TestId, x => x.Count, ct);

        return tests.Select(t => ToSummary(t, counts.GetValueOrDefault(t.Id))).ToList();
    }

    public async Task<TestResultsDto> GetResultsAsync(Guid testId, CancellationToken ct = default)
    {
        var test = await _db.Tests
            .AsNoTracking()
            .Include(t => t.Blocks)
            .Include(t => t.Group)
            .FirstOrDefaultAsync(t => t.Id == testId, ct)
            ?? throw new NotFoundException("Test", testId);

        var attempts = await _db.TestAttempts
            .AsNoTracking()
            .Where(a => a.TestId == testId)
            .Include(a => a.Student).ThenInclude(s => s.User)
            .OrderByDescending(a => a.TotalScore)
            .ToListAsync(ct);

        var maxScore = GradingHelper.MaxScore(test);

        var attemptDtos = attempts.Select(a => new AttemptResultDto(
            a.Id,
            a.Student.User.FullName,
            a.SubmittedAt,
            a.CorrectCount,
            test.QuestionCount,
            a.TotalScore,
            maxScore,
            a.Status.ToString())).ToList();

        // Savol bo'yicha tahlil
        var submitted = attempts.Where(a => a.Status == AttemptStatus.Submitted).ToList();
        var questionStats = new List<QuestionStatDto>();
        for (int i = 0; i < test.QuestionCount; i++)
        {
            char correctOption = i < test.AnswerKey.Length ? test.AnswerKey[i] : '?';
            int correct = submitted.Count(a => i < a.Answers.Length && a.Answers[i] == correctOption);
            questionStats.Add(new QuestionStatDto(i + 1, correctOption, correct, submitted.Count));
        }

        return new TestResultsDto(ToSummary(test, submitted.Count), attemptDtos, questionStats);
    }

    public async Task DeleteAsync(Guid testId, CancellationToken ct = default)
    {
        var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == testId, ct)
            ?? throw new NotFoundException("Test", testId);
        _db.Tests.Remove(test);
        await _db.SaveChangesAsync(ct);
    }

    // ===================== Sayt — talaba =====================
    public async Task<IReadOnlyList<StudentTestResultDto>> GetMyResultsAsync(Guid userId, CancellationToken ct = default)
    {
        var studentId = await _db.Students
            .Where(s => s.UserId == userId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);

        if (studentId is null) return Array.Empty<StudentTestResultDto>();

        var attempts = await _db.TestAttempts
            .AsNoTracking()
            .Where(a => a.StudentId == studentId.Value)
            .Include(a => a.Test).ThenInclude(t => t.Blocks)
            .Include(a => a.Test).ThenInclude(t => t.Group)
            .OrderByDescending(a => a.Test.StartsAt)
            .ToListAsync(ct);

        return attempts.Select(a => new StudentTestResultDto(
            a.TestId,
            a.Test.Title,
            a.Test.StartsAt,
            a.Test.Group != null ? a.Test.Group.Name : null,
            a.CorrectCount,
            a.Test.QuestionCount,
            a.TotalScore,
            GradingHelper.MaxScore(a.Test),
            a.Status.ToString(),
            a.Answers,
            a.Test.AnswerKey)).ToList();
    }

    // ===================== Bot — topshirish =====================
    public async Task<ActiveTestDto?> GetActiveTestForStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var activeTests = await _db.Tests
            .AsNoTracking()
            .Where(t => t.Status == TestStatus.Active)
            .ToListAsync(ct);

        foreach (var test in activeTests.OrderBy(t => t.StartsAt))
        {
            if (now < test.StartsAt || now >= test.EndsAt) continue;
            if (!await IsEligibleAsync(test, studentId, ct)) continue;

            var alreadySubmitted = await _db.TestAttempts.AnyAsync(
                a => a.TestId == test.Id && a.StudentId == studentId && a.Status == AttemptStatus.Submitted, ct);
            if (alreadySubmitted) continue;

            return new ActiveTestDto(test.Id, test.Title, test.TelegramFileId, test.QuestionCount, test.EndsAt);
        }
        return null;
    }

    public async Task<SubmitResult> SubmitAsync(Guid testId, Guid studentId, string rawAnswers, CancellationToken ct = default)
    {
        var test = await _db.Tests
            .Include(t => t.Blocks)
            .FirstOrDefaultAsync(t => t.Id == testId, ct)
            ?? throw new NotFoundException("Test", testId);

        var now = DateTime.UtcNow;
        if (test.Status != TestStatus.Active || now < test.StartsAt || now >= test.EndsAt)
            throw new AppException("Test hozir faol emas yoki vaqti tugagan.");

        if (!await IsEligibleAsync(test, studentId, ct))
            throw new ForbiddenException("Siz bu testni topshira olmaysiz.");

        var existing = await _db.TestAttempts
            .FirstOrDefaultAsync(a => a.TestId == testId && a.StudentId == studentId, ct);
        if (existing is { Status: AttemptStatus.Submitted })
            throw new ConflictException("Siz bu testni allaqachon topshirib bo'lgansiz.");

        var normalized = GradingHelper.Normalize(rawAnswers, test.OptionCount);
        var (correct, score, wrong) = GradingHelper.Grade(test, normalized);

        if (existing is null)
        {
            existing = new TestAttempt
            {
                TestId = testId,
                StudentId = studentId,
                StartedAt = now,
            };
            _db.TestAttempts.Add(existing);
        }

        existing.Answers = normalized;
        existing.CorrectCount = correct;
        existing.TotalScore = score;
        existing.Status = AttemptStatus.Submitted;
        existing.SubmittedAt = now;
        await _db.SaveChangesAsync(ct);

        return new SubmitResult(correct, test.QuestionCount, score, GradingHelper.MaxScore(test), wrong);
    }

    // ===================== Bot — scheduler =====================
    public async Task<IReadOnlyList<DueTestDto>> StartDueTestsAsync(DateTime nowUtc, CancellationToken ct = default)
    {
        var due = await _db.Tests
            .Where(t => t.Status == TestStatus.Scheduled && t.StartsAt <= nowUtc)
            .ToListAsync(ct);

        foreach (var t in due)
            t.Status = TestStatus.Active;

        if (due.Count > 0)
            await _db.SaveChangesAsync(ct);

        return due.Select(t => new DueTestDto(t.Id, t.Title, t.TelegramFileId, t.QuestionCount, t.DurationMinutes)).ToList();
    }

    public async Task<IReadOnlyList<Guid>> FinishDueTestsAsync(DateTime nowUtc, CancellationToken ct = default)
    {
        var active = await _db.Tests
            .Where(t => t.Status == TestStatus.Active)
            .ToListAsync(ct);

        var finished = active.Where(t => t.EndsAt <= nowUtc).ToList();
        if (finished.Count == 0) return Array.Empty<Guid>();

        var ids = finished.Select(t => t.Id).ToList();
        foreach (var t in finished)
            t.Status = TestStatus.Finished;

        // Topshirmaganlarni "vaqt tugadi" deb belgilash
        var pending = await _db.TestAttempts
            .Where(a => ids.Contains(a.TestId) && a.Status == AttemptStatus.InProgress)
            .ToListAsync(ct);
        foreach (var a in pending)
            a.Status = AttemptStatus.Expired;

        await _db.SaveChangesAsync(ct);
        return ids;
    }

    public async Task<IReadOnlyList<RecipientDto>> GetRecipientsAsync(Guid testId, CancellationToken ct = default)
    {
        var test = await _db.Tests.AsNoTracking().FirstOrDefaultAsync(t => t.Id == testId, ct)
            ?? throw new NotFoundException("Test", testId);

        IQueryable<Student> students = _db.Students.AsNoTracking().Where(s => s.User.TelegramChatId != null);

        if (test.GroupId is Guid groupId)
        {
            var studentIds = _db.Enrollments
                .Where(e => e.GroupId == groupId && e.Status != EnrollmentStatus.Cancelled)
                .Select(e => e.StudentId);
            students = students.Where(s => studentIds.Contains(s.Id));
        }

        return await students
            .Select(s => new RecipientDto(s.Id, s.User.TelegramChatId!.Value, s.User.FullName))
            .ToListAsync(ct);
    }

    // ===================== Yordamchilar =====================
    private async Task<bool> IsEligibleAsync(Test test, Guid studentId, CancellationToken ct)
    {
        if (test.GroupId is null) return true;
        return await _db.Enrollments.AnyAsync(
            e => e.GroupId == test.GroupId && e.StudentId == studentId && e.Status != EnrollmentStatus.Cancelled, ct);
    }

    private static TestSummaryDto ToSummary(Test test, int submittedCount) => new(
        test.Id,
        test.Title,
        test.QuestionCount,
        test.StartsAt,
        test.DurationMinutes,
        test.Status.ToString(),
        test.Group?.Name,
        submittedCount,
        GradingHelper.MaxScore(test));
}
