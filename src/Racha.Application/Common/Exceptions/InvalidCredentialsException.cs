namespace Racha.Application.Common.Exceptions;

/// <summary>
/// Login failed. The message is intentionally the same for an unknown e-mail and a wrong password,
/// so attackers can't find out which e-mails are registered.
/// </summary>
public sealed class InvalidCredentialsException() : Exception("Invalid email or password.");
