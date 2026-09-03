using MediatR;
using Nailify.Application.Common.Exceptions;
using Nailify.Application.Common.Interfaces;
using Nailify.Contracts.Auth;
using Nailify.Domain.Entities;
using Nailify.Domain.Enums;

namespace Nailify.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenIssuer _authTokenIssuer;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenIssuer authTokenIssuer)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _authTokenIssuer = authTokenIssuer;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
            throw new ConflictException("Email is already registered.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber.Trim(),
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return await _authTokenIssuer.IssueTokensAsync(user, cancellationToken);
    }
}
