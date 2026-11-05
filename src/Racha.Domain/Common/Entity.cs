namespace Racha.Domain.Common;

/// <summary>Base type for aggregates and entities identified by a <see cref="Guid"/>.</summary>
public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
}
