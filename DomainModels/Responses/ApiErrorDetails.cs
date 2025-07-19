// Ignore Spelling: Api

namespace Domain.Models.Responses;
public class ApiErrorDetails
{
    public string Title { get; init; } = "An error occurred";
    public string? Detail { get; init; }
    public string Instance { get; init; } = "/";
    public int Status { get; init; }
}

