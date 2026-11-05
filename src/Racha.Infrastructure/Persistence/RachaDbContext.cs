using Microsoft.EntityFrameworkCore;
using Npgsql;
using Racha.Application.Abstractions.Persistence;
using Racha.Application.Common.Exceptions;
using Racha.Domain.Expenses;
using Racha.Domain.Groups;
using Racha.Domain.Users;

namespace Racha.Infrastructure.Persistence;

public sealed class RachaDbContext(DbContextOptions<RachaDbContext> options, TimeProvider timeProvider)
    : DbContext(options), IUnitOfWork
{
    /// <summary>Audit column kept as an EF shadow property, so the domain entities don't need to know about it.</summary>
    public const string CreatedAtColumn = "CreatedAt";

    public DbSet<User> Users => Set<User>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<Expense> Expenses => Set<Expense>();

    /// <summary>
    /// Saves all tracked changes in a single transaction. A unique-key violation
    /// (e.g. two sign-ups racing for the same e-mail) becomes a <see cref="ConflictException"/>.
    /// </summary>
    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new ConflictException(pg.ConstraintName == "ix_users_email"
                ? "An account with this email already exists."
                : "The data conflicts with an existing record.");
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampCreatedAt();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampCreatedAt();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RachaDbContext).Assembly);

    private void StampCreatedAt()
    {
        var now = timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Metadata.FindProperty(CreatedAtColumn) is not null)
            {
                entry.Property(CreatedAtColumn).CurrentValue = now;
            }
        }
    }
}
