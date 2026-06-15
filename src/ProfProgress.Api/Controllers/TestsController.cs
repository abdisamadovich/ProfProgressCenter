using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfProgress.Application.Features.Tests;

namespace ProfProgress.Api.Controllers;

[Authorize]
public class TestsController : ApiControllerBase
{
    private readonly ITestService _service;

    public TestsController(ITestService service) => _service = service;

    /// <summary>Joriy o'quvchining test natijalari.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<StudentTestResultDto>>> My(CancellationToken ct)
        => Ok(await _service.GetMyResultsAsync(CurrentUserId, ct));

    /// <summary>Barcha testlar (admin/o'qituvchi).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<IReadOnlyList<TestSummaryDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    /// <summary>Test bo'yicha natijalar va tahlil.</summary>
    [HttpGet("{id:guid}/results")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<TestResultsDto>> Results(Guid id, CancellationToken ct)
        => Ok(await _service.GetResultsAsync(id, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
