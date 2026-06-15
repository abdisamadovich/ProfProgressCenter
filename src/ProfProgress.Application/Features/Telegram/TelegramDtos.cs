using ProfProgress.Domain.Enums;

namespace ProfProgress.Application.Features.Telegram;

public record LinkedUserDto(
    Guid UserId,
    Guid? StudentId,
    UserRole Role,
    string FullName,
    bool IsStaff);
