using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfProgress.Application.Features.People;

namespace ProfProgress.Api.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class TeachersController : ApiControllerBase
{
    private readonly ITeacherService _service;

    public TeachersController(ITeacherService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TeacherDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpPost]
    public async Task<ActionResult<TeacherDto>> Create(CreateTeacherRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));
}