// Ignore Spelling: app Middleware unhandled leddy dev

// -----------------------------------------------------------------------------
// File: ExceptionMiddleware.cs
// Summary: Implements centralized global exception handling middleware for the
//          Tournament API. Converts exceptions into standardized HTTP ProblemDetails
//          responses with appropriate status codes and messages, including special
//          handling for known domain exceptions like TournamentNotFoundException.
// Author: [Clive Leddy]
// Created: [2025-07-13]
// Last updated: [2025-08-02]
// Notes: Uses ASP.NET Core's built-in ProblemDetailsFactory for creating RFC 7807
//        compliant error responses and adapts output based on environment (dev vs prod).
// -----------------------------------------------------------------------------


using Domain.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Tournaments.Api.Extensions;

/// <summary>
/// Provides extension methods to configure global exception handling middleware
/// for the web application pipeline.
/// </summary>
public static class ExceptionMiddleware
{
    /// <summary>
    /// Configures the global exception handler middleware to catch unhandled exceptions,
    /// map known exceptions to appropriate HTTP status codes, and return
    /// ProblemDetails-formatted JSON responses.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    public static void ConfigureExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var contextFeatures = context.Features.Get<IExceptionHandlerFeature>();
                if(contextFeatures is not null) {
                    var problemDetailsFactory = app.Services.GetService<ProblemDetailsFactory>();
                    ArgumentNullException.ThrowIfNull(nameof(problemDetailsFactory));

                    var problemDetails = CreateProblemDetails(context, contextFeatures.Error, problemDetailsFactory, app);

                    context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;//

                    // Serialize the problem details to JSON and write it to the response
                    await context.Response.WriteAsJsonAsync(problemDetails);
                }
            });
        });
    }


    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance based on the exception type,
    /// setting the HTTP status code, title, detail message, and request path.
    /// Known exceptions like <see cref="TournamentNotFoundException"/> return 404 responses.
    /// Unhandled exceptions return a generic 500 response with optional detailed messages
    /// in development environments.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="error">The caught exception.</param>
    /// <param name="problemDetailsFactory">Factory to create ProblemDetails instances.</param>
    /// <param name="app">The <see cref="WebApplication"/> for environment info.</param>
    /// <returns>A configured <see cref="ProblemDetails"/> instance for the response.</returns>
    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception error, ProblemDetailsFactory? problemDetailsFactory, WebApplication app)
    {

        return error switch
        {
            TournamentNotFoundException tournamentNotFoundException => problemDetailsFactory!.CreateProblemDetails(
                context,
                StatusCodes.Status404NotFound,
                title: tournamentNotFoundException.Title,
                detail: tournamentNotFoundException.Message,
                instance: context.Request.Path),
            _ => problemDetailsFactory!.CreateProblemDetails(
                context,
                StatusCodes.Status500InternalServerError,
                title: "Internal server error occurred.",
                detail: app.Environment.IsDevelopment() ? error.Message :
                    "An unexpected error occurred.")
        };
    }

}
