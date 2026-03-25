namespace MiniPos.Frontend.Models;

public class PaginationFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "OrderDate";
    public bool IsDecending { get; set; } = true;
}
