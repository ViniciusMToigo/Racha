using Racha.Domain.Users;

namespace Racha.Application.Users;

public sealed record UserDto(Guid Id, string Name, string Email)
{
    public static UserDto From(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new UserDto(user.Id, user.Name, user.Email);
    }
}
