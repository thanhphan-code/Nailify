using Nailify.Contracts.Auth;
using Nailify.Domain.Entities;

namespace Nailify.Application.Auth;

public static class UserMapper
{
    public static UserDto ToDto(User user) => new(
        user.Id,
        user.FullName,
        user.Email,
        user.PhoneNumber,
        user.Role.ToString(),
        user.Status.ToString());
}
