using MediatR;
using Nailify.Application.Auth;
using Nailify.Application.Common.Exceptions;
using Nailify.Application.Common.Interfaces;
using Nailify.Contracts.Auth;

namespace Nailify.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return UserMapper.ToDto(user);
    }
}
