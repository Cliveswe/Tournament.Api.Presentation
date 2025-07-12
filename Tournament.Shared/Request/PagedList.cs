using Microsoft.EntityFrameworkCore;

namespace Tournaments.Shared.Request;
public class PagedList<T>(IEnumerable<T> items, int count, int pageNumber, int pageSize)
{
    public IReadOnlyList<T> Items { get; } = items.ToList();
    public MetaData MetaData { get; } = new MetaData(
        currentPage: pageNumber,
        totalPages: (int)Math.Ceiling(count / (double)pageSize),
        pageSize: pageSize,
        totalCount: count
        );

    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
