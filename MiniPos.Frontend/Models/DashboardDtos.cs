namespace MiniPos.Frontend.Models;

public class DashboardResponseDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalMerchants { get; set; }
    public int TotalBranches { get; set; }
    public List<OrderListResponseDto> RecentOrders { get; set; } = new();
    public List<DailySaleDto> DailySales { get; set; } = new();
}

public class DailySaleDto
{
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderListRequestDto : PaginationFilter
{
    public Guid? BranchId { get; set; }
    public Guid? ProcessedById { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
}

public class OrderListResponseDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ProcessedById { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemListDto> OrderItems { get; set; } = new();
}

public class OrderItemListDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
