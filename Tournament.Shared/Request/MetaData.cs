namespace Tournaments.Shared.Request;
public class MetaData(int currentPage, int totalPages, int pageSize, int totalCount)
{
    public int CurrentPage { get; } = currentPage;
    public int TotalPages { get; } = totalPages;
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = totalCount;
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
