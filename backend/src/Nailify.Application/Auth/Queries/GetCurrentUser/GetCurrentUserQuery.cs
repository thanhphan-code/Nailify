using MediatR;
using Nailify.Contracts.Auth;

namespace Nailify.Application.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;
