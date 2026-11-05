using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Racha.Application.Auth;
using Racha.Application.Users;

namespace Racha.Auth.Api.Tests;

[Collection(AuthApiCollection.Name)]
public class AuthEndpointsTests(AuthApiFactory factory)
{
    private const string Password = "Passw0rd!";

    private readonly HttpClient _client = factory.CreateClient();

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@test.dev";

    private async Task<AuthResponse> RegisterAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Alice", email, Password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    [Fact]
    public async Task Health_IsPublic_Returns200()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithValidData_Returns201WithTokenAndLocation()
    {
        var email = UniqueEmail();

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Alice", email, Password));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should().EndWith("/api/auth/me");
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409Problem()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Bob", email, Password));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400WithFieldErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Alice", UniqueEmail(), "weak"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("password");
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<AuthResponse>())!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401Problem()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassw0rd"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithTamperedToken_Returns401()
    {
        var auth = await RegisterAsync(UniqueEmail());
        var tampered = auth.AccessToken[..^4] + "AAAA";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tampered);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsTheCurrentUser()
    {
        var email = UniqueEmail();
        var auth = await RegisterAsync(email);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<UserDto>();
        me!.Id.Should().Be(auth.User.Id);
        me.Email.Should().Be(email);
    }
}
