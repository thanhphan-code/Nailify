using MediatR;
using Nailify.Contracts.Auth;

namespace Nailify.Application.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
