namespace ProfProgress.Application.Features.Grades;

public interface IGradeService
{
    Task<GradeDto> CreateAsync(CreateGradeRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<GradeDto>> GetByGroupAsync(Guid groupId, CancellationToken ct = default);

    Task<IReadOnlyList<GradeDto>> GetMyGradesAsync(Guid userId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}