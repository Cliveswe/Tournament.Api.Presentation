using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Responses;
public class ApiErrorDetails
{
    public string Title { get; init; } = "An error occurred";
    public string? Detail { get; init; }
    public string Instance { get; init; } = "/";
    public int Status { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

