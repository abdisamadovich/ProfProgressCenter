using Microsoft.Extensions.DependencyInjection;
using ProfProgress.Application.Features.Attendance;
using ProfProgress.Application.Features.Auth;
using ProfProgress.Application.Features.Courses;
using ProfProgress.Application.Features.Enrollments;
using ProfProgress.Application.Features.Grades;
using ProfProgress.Application.Features.Groups;
using ProfProgress.Application.Features.People;
using ProfProgress.Application.Features.Telegram;
using ProfProgress.Application.Features.Tests;

namespace ProfProgress.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ITestService, TestService>();
        services.AddScoped<ITelegramAccountService, TelegramAccountService>();

        return services;
    }
}