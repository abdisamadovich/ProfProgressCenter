namespace ProfProgress.Application.Features.People;

public interface IStudentService
{
    Task<IReadOnlyList<StudentDto>> GetAllAsync(string? search, CancellationToken ct = default);

    Task<StudentDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<StudentDto> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}

public interface ITeacherService
{
    Task<IReadOnlyList<TeacherDto>> GetAllAsync(CancellationToken ct = default);

    Task<TeacherDto> CreateAsync(CreateTeacherRequest request, CancellationToken ct = default);
}