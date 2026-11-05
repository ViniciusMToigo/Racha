namespace Racha.Application.Common.Exceptions;

/// <summary>
/// The resource does not exist or the current user is not allowed to see it.
/// Using the same error for both avoids revealing that a resource exists.
/// </summary>
public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} '{key}' was not found.");
