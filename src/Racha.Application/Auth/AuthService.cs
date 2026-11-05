using FluentValidation;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Abstractions.Security;
using Racha.Application.Users;

namespace Racha.Application.Auth;

// Red phase only: the dependencies are not used yet. Removed when the service is implemented.
#pragma warning disable CS9113 // Parameter is unread
public sealed class AuthService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : IAuthService
{
    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
#pragma warning restore CS9113
