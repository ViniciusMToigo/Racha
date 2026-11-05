using Microsoft.EntityFrameworkCore;
using Racha.Application.Abstractions.Security;
using Racha.Domain.Expenses;
using Racha.Domain.Groups;
using Racha.Domain.Users;

namespace Racha.Infrastructure.Persistence;

/// <summary>Demo credentials, also documented in the README.</summary>
public static class DemoData
{
    public const string Password = "Demo123!";
    public const string AliceEmail = "alice@racha.dev";
    public const string BobEmail = "bob@racha.dev";
    public const string CarolEmail = "carol@racha.dev";
    public const string TripGroupName = "Trip to Floripa";
    public const string ApartmentGroupName = "Apartment 42";
}

/// <summary>Creates demo users, groups and expenses. Safe to run many times: it only seeds once.</summary>
/// <remarks>
/// Entities are created through the domain factories (not raw SQL), so demo data obeys the same
/// business rules as real data, e.g. expenses are split with the real <see cref="EqualSplitter"/>.
/// </remarks>
public sealed class DatabaseSeeder(RachaDbContext db, IPasswordHasher passwordHasher, TimeProvider timeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Checks for the demo user rather than "any user", so it also works on a database that already has data.
        if (await db.Users.AnyAsync(u => u.Email == DemoData.AliceEmail, cancellationToken))
        {
            return;
        }

        // Hash once: PBKDF2 is slow on purpose and the three demo users share the password.
        var passwordHash = passwordHasher.Hash(DemoData.Password);
        var alice = User.Create("Alice", DemoData.AliceEmail, passwordHash);
        var bob = User.Create("Bob", DemoData.BobEmail, passwordHash);
        var carol = User.Create("Carol", DemoData.CarolEmail, passwordHash);

        var trip = Group.Create(DemoData.TripGroupName, alice.Id);
        trip.AddMember(bob.Id);
        trip.AddMember(carol.Id);

        var apartment = Group.Create(DemoData.ApartmentGroupName, bob.Id);
        apartment.AddMember(alice.Id);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        db.Users.AddRange(alice, bob, carol);
        db.Groups.AddRange(trip, apartment);
        db.Expenses.AddRange(
            Expense.Create(trip, "Airbnb (3 nights)", 1200.00m, today.AddDays(-10), alice.Id),
            Expense.Create(trip, "Dinner at the beach", 345.60m, today.AddDays(-9), bob.Id),
            Expense.Create(trip, "Car rental", 480.00m, today.AddDays(-8), carol.Id),
            Expense.Create(trip, "Groceries", 213.37m, today.AddDays(-7), alice.Id),
            Expense.Create(apartment, "Internet bill", 120.00m, today.AddDays(-3), bob.Id),
            Expense.Create(apartment, "Cleaning supplies", 57.90m, today.AddDays(-1), alice.Id));

        await db.SaveChangesAsync(cancellationToken);
    }
}
