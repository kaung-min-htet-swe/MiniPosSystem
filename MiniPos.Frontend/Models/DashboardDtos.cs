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

public class OrderListResponseDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ProcessedById { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
}
