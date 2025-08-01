// -------------------------------------------------------------------------------------
// File: PagedList.cs
// Summary: Represents a paged collection of items along with pagination metadata,
//          including total item count and current page information.
// <author> [Clive Leddy] </author>
// <created> [2025-07-21] </created>
// Notes: Used to support pagination in API responses. Includes an asynchronous factory
//        method for creating paged results directly from an IQueryable data source.
// -------------------------------------------------------------------------------------


using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Tournaments.Shared.Request;

/// <summary>
/// Represents a paginated list of items and accompanying metadata for API pagination support.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[SwaggerSchema("Represents a paginated set of data along with metadata.")]
public class PagedList<T>(IEnumerable<T> items, int count, int pageNumber, int pageSize)
{
    /// <summary>
    /// Gets the read-only list of items on the current page.
    /// </summary>
    [SwaggerSchema("The items contained on the current page of results.")]
    public IReadOnlyList<T> Items { get; } = items.ToList();

    /// <summary>
    /// Gets the pagination metadata including total count, current page, page size, and total pages.
    /// </summary>
    [SwaggerSchema("Metadata containing pagination information such as page number and total pages.")]
    public MetaData MetaData { get; } = new MetaData(
        currentPage: pageNumber,
        totalPages: (int)Math.Ceiling(count / (double)pageSize),
        pageSize: pageSize,
        totalCount: count
        );

    /// <summary>
    /// Asynchronously creates a paginated list from the given IQueryable source by applying
    /// skip/take operations and calculating pagination metadata.
    /// </summary>
    /// <param name="source">The IQueryable source of items to paginate.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A task representing the asynchronous operation, containing the paginated list.</returns>
    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source, int pageNumber, int pageSize)
    {
        // Work with the DB source IQueryable to get the total count
        var count = await source.CountAsync();

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        // Create a new instance of PagedList with the items and meta-data
        // and return it.
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
