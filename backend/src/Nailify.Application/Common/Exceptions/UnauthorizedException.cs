namespace Nailify.Application.Common.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Invalid credentials.") : base(message) { }
}
