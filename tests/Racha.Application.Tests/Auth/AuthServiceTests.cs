using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Abstractions.Security;
using Racha.Application.Auth;
using Racha.Application.Common.Exceptions;
using Racha.Domain.Users;

namespace Racha.Application.Tests.Auth;

public class AuthServiceTests
{
    private static readonly AccessToken Token = new("jwt-token", new DateTimeOffset(2026, 11, 5, 12, 0, 0, TimeSpan.Zero));
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        // Fake hasher: "secret" becomes "hashed:secret", which keeps assertions readable.
        _hasher.Hash(Arg.Any<string>()).Returns(call => $"hashed:{call.ArgAt<string>(0)}");
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => call.ArgAt<string>(1) == $"hashed:{call.ArgAt<string>(0)}");
        _tokens.CreateToken(Arg.Any<User>()).Returns(Token);

        _sut = new AuthService(
            _users, _unitOfWork, _hasher, _tokens, new RegisterRequestValidator(), new LoginRequestValidator());
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_SavesUserWithHashedPasswordAndReturnsToken()
    {
        _users.EmailExistsAsync("alice@example.com", Ct).Returns(false);

        var response = await _sut.RegisterAsync(new RegisterRequest("Alice", " Alice@Example.com ", "Passw0rd!"), Ct);

        response.AccessToken.Should().Be(Token.Token);
        response.ExpiresAt.Should().Be(Token.ExpiresAt);
        response.User.Name.Should().Be("Alice");
        response.User.Email.Should().Be("alice@example.com");
        _users.Received(1).Add(Arg.Is<User>(u => u.Email == "alice@example.com" && u.PasswordHash == "hashed:Passw0rd!"));
        await _unitOfWork.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsConflictAndSavesNothing()
    {
        _users.EmailExistsAsync("alice@example.com", Ct).Returns(true);

        var act = () => _sut.RegisterAsync(new RegisterRequest("Alice", "alice@example.com", "Passw0rd!"), Ct);

        await act.Should().ThrowAsync<ConflictException>();
        _users.DidNotReceive().Add(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidRequest_ThrowsValidationBeforeTouchingTheDatabase()
    {
        var act = () => _sut.RegisterAsync(new RegisterRequest("Alice", "alice@example.com", "weak"), Ct);

        await act.Should().ThrowAsync<ValidationException>();
        await _users.DidNotReceive().EmailExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenForThatUser()
    {
        var alice = User.Create("Alice", "alice@example.com", "hashed:Passw0rd!");
        _users.GetByEmailAsync("alice@example.com", Ct).Returns(alice);

        var response = await _sut.LoginAsync(new LoginRequest(" ALICE@example.com ", "Passw0rd!"), Ct);

        response.User.Id.Should().Be(alice.Id);
        response.AccessToken.Should().Be(Token.Token);
        _tokens.Received(1).CreateToken(alice);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var alice = User.Create("Alice", "alice@example.com", "hashed:Passw0rd!");
        _users.GetByEmailAsync("alice@example.com", Ct).Returns(alice);

        var act = () => _sut.LoginAsync(new LoginRequest("alice@example.com", "WrongPassw0rd"), Ct);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _tokens.DidNotReceive().CreateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsTheSameInvalidCredentialsError()
    {
        _users.GetByEmailAsync("nobody@example.com", Ct).Returns((User?)null);

        var act = () => _sut.LoginAsync(new LoginRequest("nobody@example.com", "Passw0rd!"), Ct);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsTheUser()
    {
        var alice = User.Create("Alice", "alice@example.com", "hash");
        _users.GetByIdAsync(alice.Id, Ct).Returns(alice);

        var profile = await _sut.GetProfileAsync(alice.Id, Ct);

        profile.Should().BeEquivalentTo(new { alice.Id, Name = "Alice", Email = "alice@example.com" });
    }

    [Fact]
    public async Task GetProfileAsync_WithUnknownUser_ThrowsNotFound()
    {
        var unknownId = Guid.NewGuid();
        _users.GetByIdAsync(unknownId, Ct).Returns((User?)null);

        var act = () => _sut.GetProfileAsync(unknownId, Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
