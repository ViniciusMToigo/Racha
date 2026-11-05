namespace Racha.Application.Abstractions.Persistence;

/// <summary>Commits every change tracked by the repositories in a single transaction.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
