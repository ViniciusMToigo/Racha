namespace Racha.Application.Common.Exceptions;

/// <summary>The request conflicts with the current state, e.g. an e-mail that is already registered.</summary>
public sealed class ConflictException(string message) : Exception(message);
