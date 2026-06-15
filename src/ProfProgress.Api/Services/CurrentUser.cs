using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Domain.Enums;
using System.Security.Claims;

namespace ProfProgress.Api.Services;

/// <summary>HTTP kontekstdagi JWT claim'laridan joriy foydalanuvchini o'qiydi.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? Principal?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var role = Principal?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(role, out var r) ? r : null;
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}