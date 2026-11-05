using NSubstitute;
using Racha.Application.Abstractions.Persistence;
using Racha.Domain.Groups;
using Racha.Domain.Users;

namespace Racha.Application.Tests;

internal static class TestData
{
    public static readonly User Alice = User.Create("Alice", "alice@example.com", "hash");
    public static readonly User Bob = User.Create("Bob", "bob@example.com", "hash");
    public static readonly User Carol = User.Create("Carol", "carol@example.com", "hash");
    public static readonly User Dave = User.Create("Dave", "dave@example.com", "hash");

    public static readonly IReadOnlyList<User> AllUsers = [Alice, Bob, Carol, Dave];

    public static readonly DateOnly Today = new(2026, 11, 4);

    public static readonly CancellationToken Ct = CancellationToken.None;

    /// <summary>A fresh group (groups are mutable, so each test builds its own).</summary>
    public static Group GroupOf(User creator, params User[] others)
    {
        var group = Group.Create("Trip to Floripa", creator.Id);
        foreach (var user in others)
        {
            group.AddMember(user.Id);
        }

        return group;
    }

    /// <summary>Makes the fake repository resolve any of the known test users by id.</summary>
    public static void ResolvesKnownUsers(this IUserRepository users) =>
        users.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var ids = call.ArgAt<IEnumerable<Guid>>(0).ToHashSet();
                return (IReadOnlyList<User>)AllUsers.Where(u => ids.Contains(u.Id)).ToList();
            });
}
