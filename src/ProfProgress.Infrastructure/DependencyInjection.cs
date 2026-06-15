using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProfProgress.Application.Common.Interfaces;
using ProfProgress.Application.Common.Settings;
using ProfProgress.Infrastructure.Persistence;
using ProfProgress.Infrastructure.Services;
using ProfProgress.Infrastructure.Telegram;

namespace ProfProgress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<TelegramSettings>(configuration.GetSection(TelegramSettings.SectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Telegram bot (token bo'lmasa o'zi o'chib qoladi)
        services.AddSingleton<ConversationStore>();
        services.AddHostedService<TelegramBotService>();

        return services;
    }
}
