using FluentValidation;
using MediatR;
using Nailify.Application.Common.Behaviors;
using Nailify.Contracts.Auth;
using Nailify.Application.Auth.Commands.Login;
using Nailify.Application.Auth.Commands.RefreshToken;
using Nailify.Application.Auth.Commands.Register;
using Nailify.Application.Auth.Queries.GetCurrentUser;
using Nailify.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Nailify.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();

        return services;
    }
}
