using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfProgress.Application.Features.Enrollments;

namespace ProfProgress.Api.Controllers;

[Authorize]
public class EnrollmentsController : ApiControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service) => _service = service;

    /// <summary>Joriy o'quvchining yozilishlari.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> My(CancellationToken ct)
        => Ok(await _service.GetMyEnrollmentsAsync(CurrentUserId, ct));

    /// <summary>O'quvchi o'zini guruhga yozadi (ariza).</summary>
    [HttpPost]
    public async Task<ActionResult<EnrollmentDto>> Enroll(EnrollRequest request, CancellationToken ct)
        => Ok(await _service.EnrollSelfAsync(CurrentUserId, request, ct));

    /// <summary>Admin boshqa o'quvchini guruhga yozadi.</summary>
    [HttpPost("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<EnrollmentDto>> AdminEnroll(AdminEnrollRequest request, CancellationToken ct)
        => Ok(await _service.AdminEnrollAsync(request, ct));

    /// <summary>Yozilish holatini o'zgartirish (tasdiqlash, bekor qilish va h.k.).</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<EnrollmentDto>> UpdateStatus(Guid id, UpdateEnrollmentStatusRequest request, CancellationToken ct)
        => Ok(await _service.UpdateStatusAsync(id, request, ct));
}