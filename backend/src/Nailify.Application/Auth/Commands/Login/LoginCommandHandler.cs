using MediatR;
using Nailify.Application.Common.Exceptions;
using Nailify.Application.Common.Interfaces;
using Nailify.Contracts.Auth;
using Nailify.Domain.Enums;

namespace Nailify.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenIssuer _authTokenIssuer;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenIssuer authTokenIssuer)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _authTokenIssuer = authTokenIssuer;
    }

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(command.Request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (user.Status != UserStatus.Active)
            throw new UnauthorizedException("Account is inactive.");

        return await _authTokenIssuer.IssueTokensAsync(user, cancellationToken);
    }
}
