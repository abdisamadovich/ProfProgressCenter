using Microsoft.AspNetCore.Mvc;
using ProfProgress.Application.Common.Interfaces;

namespace ProfProgress.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ICurrentUser? _currentUser;

    protected ICurrentUser CurrentUser =>
        _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

    /// <summary>Joriy autentifikatsiyalangan foydalanuvchi Id'si (claim'dan).</summary>
    protected Guid CurrentUserId =>
        CurrentUser.UserId ?? throw new UnauthorizedAccessException("Foydalanuvchi aniqlanmadi.");
}