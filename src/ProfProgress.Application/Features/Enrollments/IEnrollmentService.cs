namespace ProfProgress.Application.Features.Enrollments;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollSelfAsync(Guid userId, EnrollRequest request, CancellationToken ct = default);

    Task<EnrollmentDto> AdminEnrollAsync(AdminEnrollRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<EnrollmentDto>> GetMyEnrollmentsAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<EnrollmentDto>> GetByGroupAsync(Guid groupId, CancellationToken ct = default);

    Task<EnrollmentDto> UpdateStatusAsync(Guid enrollmentId, UpdateEnrollmentStatusRequest request, CancellationToken ct = default);
}