using Nailify.Contracts.Auth;
using Nailify.Domain.Entities;
using Nailify.Application.Common.Interfaces;

namespace Nailify.Application.Auth;

public class AuthTokenIssuer : IAuthTokenIssuer
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthTokenIssuer(IJwtTokenService jwtTokenService, IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken = default)
    {
        var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = _jwtTokenService.GetRefreshTokenExpiry()
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshTokenValue, expiresAt, UserMapper.ToDto(user));
    }
}
