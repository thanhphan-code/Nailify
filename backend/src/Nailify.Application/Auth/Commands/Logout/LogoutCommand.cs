using MediatR;

namespace Nailify.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
