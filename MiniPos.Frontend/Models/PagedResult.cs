namespace MiniPos.Frontend.Models;

public class PagedResult<T>
{
    public PagedResult(List<T> data, int count, int pageNumber, int pageSize)
    {
        Data = data;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public List<T> Data { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}