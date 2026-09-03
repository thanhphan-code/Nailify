using Nailify.Contracts.Auth;
using Nailify.Domain.Entities;

namespace Nailify.Application.Auth;

public interface IAuthTokenIssuer
{
    Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken = default);
}
