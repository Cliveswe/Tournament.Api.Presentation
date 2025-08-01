// Ignore Spelling: Api

// -------------------------------------------------------------------------------------
// File: ApiErrorDetails.cs
// Summary: Represents a standardized error response model for API exceptions,
//          including status code, error title, detail message, and request path.
// <author> [Clive Leddy] </author>
// <created> [2025-07-19] </created>
// Notes: Conforms to Problem Details format (RFC 7807) and is suitable for use
//        in global exception handling middle-ware or custom error responses.
// -------------------------------------------------------------------------------------

namespace Domain.Models.Responses;

/// <summary>
/// Represents detailed information about an error response,
/// including a title, optional detailed message, instance URI, and HTTP status code.
/// Used for conveying error information in API responses.
/// </summary>
public class ApiErrorDetails
{
    /// <summary>
    /// Gets the title of the error. Defaults to "An error occurred".
    /// </summary>
    public string Title { get; init; } = "An error occurred";

    /// <summary>
    /// Gets detailed information about the error. Optional.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// Gets the URI reference that identifies the specific occurrence of the error. Defaults to "/".
    /// </summary>
    public string Instance { get; init; } = "/";

    /// <summary>
    /// Gets the HTTP status code associated with the error.
    /// </summary>
    public int Status { get; init; }
}

