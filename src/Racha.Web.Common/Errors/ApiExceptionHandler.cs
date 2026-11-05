using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Racha.Application.Common.Exceptions;
using Racha.Domain.Common;

namespace Racha.Web.Common.Errors;

/// <summary>
/// Translates exceptions from the inner layers into RFC 9457 ProblemDetails responses,
/// so controllers stay free of try/catch and every error has the same JSON shape.
/// </summary>
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validation => ToValidationProblem(validation),
            DomainException => Problem(StatusCodes.Status400BadRequest, "Business rule violated", exception.Message),
            InvalidCredentialsException => Problem(StatusCodes.Status401Unauthorized, "Authentication failed", exception.Message),
            UnauthorizedAccessException => Problem(StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            ForbiddenException => Problem(StatusCodes.Status403Forbidden, "Forbidden", exception.Message),
            NotFoundException => Problem(StatusCodes.Status404NotFound, "Not found", exception.Message),
            ConflictException => Problem(StatusCodes.Status409Conflict, "Conflict", exception.Message),

            // Never leak internal details (stack traces, SQL) to the client.
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred."),
        };

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        problem.Instance = httpContext.Request.Path;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problem.Status!.Value;

        // Serialized as object so derived types (ValidationProblemDetails) keep their extra fields.
        await httpContext.Response.WriteAsJsonAsync<object>(
            problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);

        return true;
    }

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    private static ValidationProblemDetails ToValidationProblem(ValidationException exception)
    {
        // camelCase keys match the JSON property names the frontend sends.
        var errors = exception.Errors
            .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        };
    }
}
