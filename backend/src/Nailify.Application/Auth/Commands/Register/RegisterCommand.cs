using MediatR;
using Nailify.Contracts.Auth;

namespace Nailify.Application.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;
