// -------------------------------------------------------------------------------------
// File: MetaData.cs
// Summary: Represents pagination metadata returned alongside paged API results,
//          including current page, total pages, page size, and total item count.
// <author> [Clive Leddy] </author>
// <created> [2025-07-21] </created>
// Notes: Supports pagination features in API responses, such as determining whether
//        a previous or next page exists. Designed to be used with paged result sets.
// -------------------------------------------------------------------------------------


using System.ComponentModel.DataAnnotations;

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
    [Required]
    [Range(1, int.MaxValue)]
    public int CurrentPage { get; } = currentPage;

    /// <summary>
    /// Gets the total number of pages available.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TotalPages { get; } = totalPages;

    /// <summary>
    /// Gets the number of items on each page.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageSize { get; } = pageSize;

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    [Range(0, int.MaxValue)]
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
