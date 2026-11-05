using FluentValidation;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Abstractions.Security;
using Racha.Application.Common.Exceptions;
using Racha.Application.Users;
using Racha.Domain.Users;

namespace Racha.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = User.NormalizeEmail(request.Email);

        // Fast, friendly check. The unique index on Users.Email is the real guarantee against races.
        if (await users.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = User.Create(request.Name, email, passwordHasher.Hash(request.Password));
        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetByEmailAsync(User.NormalizeEmail(request.Email), cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return CreateResponse(user);
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        return UserDto.From(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var token = tokenService.CreateToken(user);
        return new AuthResponse(UserDto.From(user), token.Token, token.ExpiresAt);
    }
}
