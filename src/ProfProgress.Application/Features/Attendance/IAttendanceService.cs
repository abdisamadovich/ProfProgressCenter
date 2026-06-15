namespace ProfProgress.Application.Features.Attendance;

public interface IAttendanceService
{
    Task<LessonDto> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<LessonDto>> GetLessonsByGroupAsync(Guid groupId, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceDto>> MarkAttendanceAsync(Guid lessonId, MarkAttendanceRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceDto>> GetAttendanceByLessonAsync(Guid lessonId, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceDto>> GetMyAttendanceAsync(Guid userId, CancellationToken ct = default);
}