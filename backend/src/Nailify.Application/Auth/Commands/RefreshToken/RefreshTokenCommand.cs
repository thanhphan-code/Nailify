using MediatR;
using Nailify.Contracts.Auth;

namespace Nailify.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<AuthResponse>;
