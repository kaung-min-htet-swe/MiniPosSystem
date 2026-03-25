using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;
using MiniPos.Backend.Features.Orders;

namespace MiniPos.Backend.Features.Dashboard;

public interface IDashboardService
{
    Task<Result<DashboardResponseDto>> GetStats();
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DashboardResponseDto>> GetStats()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-7);

            var orders = await _db.Orders.AsNoTracking().ToListAsync();
            var merchantsCount = await _db.Merchants.CountAsync(m => m.DeletedAt == null);
            var branchesCount = await _db.Branches.CountAsync(b => b.DeletedAt == null);

            var revenue = orders.Sum(o => o.TotalAmount);
            var transactions = orders.Count;

            var recentOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new OrderListResponseDto
                {
                    Id = o.Id,
                    BranchId = o.BranchId,
                    ProcessedById = o.ProcessedById,
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

            var response = new DashboardResponseDto
            {
                TotalRevenue = revenue,
                TotalTransactions = transactions,
                TotalMerchants = merchantsCount,
                TotalBranches = branchesCount,
                RecentOrders = recentOrders,
                DailySales = dailySales
            };

            return Result<DashboardResponseDto>.Success(response);
        }
        catch (Exception e)
        {
            return Result<DashboardResponseDto>.Failure(new InternalError("Dashboard.GetStats", e.Message));
        }
    }
}

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
