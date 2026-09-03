namespace Nailify.Contracts.Auth;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string Role,
    string Status);
