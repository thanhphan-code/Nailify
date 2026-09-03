using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nailify.Application.Common.Interfaces;
using Nailify.Infrastructure.Auth;
using Nailify.Infrastructure.Email;
using Nailify.Infrastructure.Persistence.Repositories;

namespace Nailify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<MailjetOptions>(configuration.GetSection(MailjetOptions.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddHttpClient<IEmailSender, MailjetEmailSender>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();

        return services;
    }
}
