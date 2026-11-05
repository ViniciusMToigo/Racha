using FluentAssertions;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Common.Exceptions;
using Racha.Domain.Users;
using Racha.Infrastructure.Persistence.Repositories;

namespace Racha.Infrastructure.Tests.Persistence;

[Collection(PostgresCollection.Name)]
public class UserRepositoryTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Add_ThenGetByEmailAndById_ReturnsTheSavedUser()
    {
        var alice = NewUser("Alice");
        await using (var db = NewContext())
        {
            new UserRepository(db).Add(alice);
            await db.SaveChangesAsync(Ct);
        }

        await using var read = NewContext();
        var repository = new UserRepository(read);

        var byEmail = await repository.GetByEmailAsync(alice.Email, Ct);
        var byId = await repository.GetByIdAsync(alice.Id, Ct);

        byEmail.Should().NotBeNull();
        byEmail!.Id.Should().Be(alice.Id);
        byEmail.Name.Should().Be("Alice");
        byEmail.PasswordHash.Should().Be("hash");
        byId.Should().NotBeNull();
        byId!.Email.Should().Be(alice.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_UnknownEmail_ReturnsNull()
    {
        await using var db = NewContext();

        var user = await new UserRepository(db).GetByEmailAsync("nobody@test.dev", Ct);

        user.Should().BeNull();
    }

    [Fact]
    public async Task EmailExistsAsync_ReflectsWhetherTheEmailIsRegistered()
    {
        var alice = (await SaveUsersAsync("Alice"))[0];

        await using var db = NewContext();
        var repository = new UserRepository(db);

        (await repository.EmailExistsAsync(alice.Email, Ct)).Should().BeTrue();
        (await repository.EmailExistsAsync("nobody@test.dev", Ct)).Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsOnlyTheRequestedUsers()
    {
        var users = await SaveUsersAsync("Alice", "Bob", "Carol");

        await using var db = NewContext();
        var result = await new UserRepository(db).GetByIdsAsync([users[0].Id, users[2].Id], Ct);

        result.Select(u => u.Name).Should().BeEquivalentTo(new[] { "Alice", "Carol" });
    }

    [Fact]
    public async Task SaveChanges_WithDuplicateEmail_ThrowsConflictException()
    {
        var alice = (await SaveUsersAsync("Alice"))[0];
        var impostor = User.Create("Impostor", alice.Email, "hash");

        await using var db = NewContext();
        new UserRepository(db).Add(impostor);
        IUnitOfWork unitOfWork = db;

        var act = () => unitOfWork.SaveChangesAsync(Ct);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*email*");
    }
}
