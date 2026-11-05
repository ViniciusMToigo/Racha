namespace Racha.Domain.Common;

/// <summary>Raised when a business rule is violated.</summary>
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
