using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.ContentType = "application/problem+json";

                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature?.Error is not Exception exception) return;

                var problemDetails = CreateProblemDetails(exception, env, context);
                context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
            });
        });
    }

    private static ProblemDetails CreateProblemDetails(Exception exception, IHostEnvironment env, HttpContext context)
    {
        return exception switch
        {
            EntityNotFoundException ex => new ProblemDetails
            {
                Title = "Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Instance = context.Request.Path
            },

            EntityValidationException ex => new ValidationProblemDetails(ToModelState(ex.Errors))
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Instance = context.Request.Path
            },

            EntityConflictException ex => new ProblemDetails
            {
                Title = "Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Instance = context.Request.Path
            },

            UnauthorizedAccessException or SecurityTokenException => new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = exception.Message,
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Instance = context.Request.Path
            },

            _ => new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = env.IsDevelopment() ? exception.ToString() : "An unexpected error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Instance = context.Request.Path
            }
        };
    }

    private static ModelStateDictionary ToModelState(Dictionary<string, string> errors)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in errors)
        {
            modelState.AddModelError(error.Key, error.Value);
        }
        return modelState;
    }
}