namespace Racha.Application.Common.Exceptions;

/// <summary>The user can see the resource but is not allowed to perform this action on it.</summary>
public sealed class ForbiddenException(string message) : Exception(message);
