using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;
using MiniPos.Backend.Features.Orders;

namespace MiniPos.Backend.Features.Dashboard;

public interface IDashboardService
{
    Task<Result<DashboardResponse>> GetStats(Guid? merchantId = null);
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DashboardResponse>> GetStats(Guid? merchantId = null)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-7);

            var ordersQuery = _db.Orders.AsNoTracking();
            if (merchantId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.MerchantId == merchantId.Value);
            }
            var orders = await ordersQuery.ToListAsync();

            var merchantsCountQuery = _db.Merchants.Where(m => m.DeletedAt == null);
            if (merchantId.HasValue)
            {
                merchantsCountQuery = merchantsCountQuery.Where(m => m.Id == merchantId.Value);
            }
            var merchantsCount = await merchantsCountQuery.CountAsync();

            var branchesCountQuery = _db.Branches.Where(b => b.DeletedAt == null);
            if (merchantId.HasValue)
            {
                branchesCountQuery = branchesCountQuery.Where(b => b.MerchantId == merchantId.Value);
            }
            var branchesCount = await branchesCountQuery.CountAsync();

            var revenue = orders.Sum(o => o.TotalAmount);
            var transactions = orders.Count;

            var recentOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new OrderListResponse
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount
                })
                .ToList();

            var dailySales = new List<DailySaleDto>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dailyTotal = orders
                    .Where(o => o.OrderDate.Date == date)
                    .Sum(o => o.TotalAmount);
                
                dailySales.Add(new DailySaleDto 
                { 
                    Date = date, 
                    TotalAmount = dailyTotal 
                });
            }

            var response = new DashboardResponse
            {
                TotalRevenue = revenue,
                TotalTransactions = transactions,
                TotalMerchants = merchantsCount,
                TotalBranches = branchesCount,
                RecentOrders = recentOrders,
                DailySales = dailySales
            };

            return Result<DashboardResponse>.Success(response);
        }
        catch (Exception e)
        {
            return Result<DashboardResponse>.Failure(new InternalError("Dashboard.GetStats", e.Message));
        }
    }
}

public class DashboardResponse
{
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalMerchants { get; set; }
    public int TotalBranches { get; set; }
    public List<OrderListResponse> RecentOrders { get; set; } = new();
    public List<DailySaleDto> DailySales { get; set; } = new();
}

public class DailySaleDto
{
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
}
