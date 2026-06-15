using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfProgress.Application.Features.Enrollments;
using ProfProgress.Application.Features.Groups;

namespace ProfProgress.Api.Controllers;

[Authorize]
public class GroupsController : ApiControllerBase
{
    private readonly IGroupService _service;
    private readonly IEnrollmentService _enrollments;

    public GroupsController(IGroupService service, IEnrollmentService enrollments)
    {
        _service = service;
        _enrollments = enrollments;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GroupDto>>> GetAll([FromQuery] Guid? courseId, CancellationToken ct)
        => Ok(await _service.GetAllAsync(courseId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GroupDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    /// <summary>Guruhdagi o'quvchilar (yozilishlar) ro'yxati.</summary>
    [HttpGet("{id:guid}/enrollments")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> GetEnrollments(Guid id, CancellationToken ct)
        => Ok(await _enrollments.GetByGroupAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<GroupDto>> Create(CreateGroupRequest request, CancellationToken ct)
    {
        var group = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<GroupDto>> Update(Guid id, UpdateGroupRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}