namespace ProfProgress.Application.Features.Courses;

public interface ICourseService
{
    Task<IReadOnlyList<CourseDto>> GetAllAsync(bool onlyActive, CancellationToken ct = default);

    Task<CourseDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<CourseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct = default);

    Task<CourseDto> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}