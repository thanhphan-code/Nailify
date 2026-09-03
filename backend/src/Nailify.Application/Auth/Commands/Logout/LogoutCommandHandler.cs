using MediatR;
using Nailify.Application.Common.Interfaces;

namespace Nailify.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return;

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked)
            return;

        await _refreshTokenRepository.RevokeAsync(storedToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
}
