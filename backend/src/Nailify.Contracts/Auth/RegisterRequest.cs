namespace Nailify.Contracts.Auth;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string PhoneNumber);
