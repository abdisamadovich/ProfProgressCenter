using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfProgress.Application.Features.Grades;

namespace ProfProgress.Api.Controllers;

[Authorize]
public class GradesController : ApiControllerBase
{
    private readonly IGradeService _service;

    public GradesController(IGradeService service) => _service = service;

    /// <summary>Joriy o'quvchining baholari.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<GradeDto>>> My(CancellationToken ct)
        => Ok(await _service.GetMyGradesAsync(CurrentUserId, ct));

    /// <summary>Guruh bo'yicha baholar.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<IReadOnlyList<GradeDto>>> GetByGroup([FromQuery] Guid groupId, CancellationToken ct)
        => Ok(await _service.GetByGroupAsync(groupId, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<GradeDto>> Create(CreateGradeRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}