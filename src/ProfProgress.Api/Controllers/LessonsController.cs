using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfProgress.Application.Features.Attendance;

namespace ProfProgress.Api.Controllers;

[Authorize]
public class LessonsController : ApiControllerBase
{
    private readonly IAttendanceService _service;

    public LessonsController(IAttendanceService service) => _service = service;

    /// <summary>Guruhdagi darslar ro'yxati.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LessonDto>>> GetByGroup([FromQuery] Guid groupId, CancellationToken ct)
        => Ok(await _service.GetLessonsByGroupAsync(groupId, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<LessonDto>> Create(CreateLessonRequest request, CancellationToken ct)
        => Ok(await _service.CreateLessonAsync(request, ct));

    /// <summary>Dars uchun davomat yozuvlari.</summary>
    [HttpGet("{lessonId:guid}/attendance")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<IReadOnlyList<AttendanceDto>>> GetAttendance(Guid lessonId, CancellationToken ct)
        => Ok(await _service.GetAttendanceByLessonAsync(lessonId, ct));

    /// <summary>Dars uchun butun guruh davomatini belgilash/yangilash.</summary>
    [HttpPost("{lessonId:guid}/attendance")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<IReadOnlyList<AttendanceDto>>> MarkAttendance(Guid lessonId, MarkAttendanceRequest request, CancellationToken ct)
        => Ok(await _service.MarkAttendanceAsync(lessonId, request, ct));
}