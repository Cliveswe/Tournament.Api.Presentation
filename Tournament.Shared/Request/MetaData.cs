namespace Tournaments.Shared.Request;

/// <summary>
/// Represents pagination metadata returned alongside paged API results.
/// </summary>
/// <param name="currentPage">The current page number in the result set.</param>
/// <param name="totalPages">The total number of available pages.</param>
/// <param name="pageSize">The number of items per page.</param>
/// <param name="totalCount">The total number of items across all pages.</param>
public class MetaData(int currentPage, int totalPages, int pageSize, int totalCount)
{
    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int CurrentPage { get; } = currentPage;

    /// <summary>
    /// Gets the total number of pages available.
    /// </summary>
    public int TotalPages { get; } = totalPages;

    /// <summary>
    /// Gets the number of items on each page.
    /// </summary>
    public int PageSize { get; } = pageSize;

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; } = totalCount;

    /// <summary>
    /// Indicates whether there is a previous page.
    /// </summary>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>
    /// Indicates whether there is a next page.
    /// </summary>
    public bool HasNext => CurrentPage < TotalPages;
}
