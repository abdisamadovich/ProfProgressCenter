using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfProgress.Application.Features.Attendance;
using ProfProgress.Application.Features.Grades;
using ProfProgress.Application.Features.People;

namespace ProfProgress.Api.Controllers;

[Authorize]
public class StudentsController : ApiControllerBase
{
    private readonly IStudentService _students;
    private readonly IAttendanceService _attendance;
    private readonly IGradeService _grades;

    public StudentsController(IStudentService students, IAttendanceService attendance, IGradeService grades)
    {
        _students = students;
        _attendance = attendance;
        _grades = grades;
    }

    /// <summary>O'quvchilar ro'yxati (admin uchun, qidiruv bilan).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll([FromQuery] string? search, CancellationToken ct)
        => Ok(await _students.GetAllAsync(search, ct));

    /// <summary>Joriy o'quvchining profili.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<StudentDto>> Me(CancellationToken ct)
        => Ok(await _students.GetByUserIdAsync(CurrentUserId, ct));

    /// <summary>Joriy o'quvchining davomati.</summary>
    [HttpGet("me/attendance")]
    public async Task<ActionResult<IReadOnlyList<AttendanceDto>>> MyAttendance(CancellationToken ct)
        => Ok(await _attendance.GetMyAttendanceAsync(CurrentUserId, ct));

    /// <summary>Joriy o'quvchining baholari.</summary>
    [HttpGet("me/grades")]
    public async Task<ActionResult<IReadOnlyList<GradeDto>>> MyGrades(CancellationToken ct)
        => Ok(await _grades.GetMyGradesAsync(CurrentUserId, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _students.GetByIdAsync(id, ct));
}