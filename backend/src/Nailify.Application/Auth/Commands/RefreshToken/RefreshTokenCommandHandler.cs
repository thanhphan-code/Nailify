using MediatR;
using Nailify.Application.Common.Exceptions;
using Nailify.Application.Common.Interfaces;
using Nailify.Contracts.Auth;
using Nailify.Domain.Enums;

namespace Nailify.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenIssuer _authTokenIssuer;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IAuthTokenIssuer authTokenIssuer)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _authTokenIssuer = authTokenIssuer;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(
            command.Request.RefreshToken, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null || user.Status != UserStatus.Active)
            throw new UnauthorizedException("User account is not available.");

        await _refreshTokenRepository.RevokeAsync(storedToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return await _authTokenIssuer.IssueTokensAsync(user, cancellationToken);
    }
}
