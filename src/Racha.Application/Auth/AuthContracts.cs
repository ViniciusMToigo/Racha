using Racha.Application.Users;

namespace Racha.Application.Auth;

public sealed record RegisterRequest(string Name, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(UserDto User, string AccessToken, DateTimeOffset ExpiresAt);
