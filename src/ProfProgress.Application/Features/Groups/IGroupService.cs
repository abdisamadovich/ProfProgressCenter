namespace ProfProgress.Application.Features.Groups;

public interface IGroupService
{
    Task<IReadOnlyList<GroupDto>> GetAllAsync(Guid? courseId, CancellationToken ct = default);

    Task<GroupDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<GroupDto> CreateAsync(CreateGroupRequest request, CancellationToken ct = default);

    Task<GroupDto> UpdateAsync(Guid id, UpdateGroupRequest request, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}