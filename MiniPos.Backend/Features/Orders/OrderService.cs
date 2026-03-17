using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace MiniPos.Backend.Features.Orders;

public interface IOrderService
{
    Task<Result<PagedResult<OrderListResponseDto>>> GetList(OrderListRequestDto request);
    Task<Result<OrderGetByIdResponseDto>> GetById(Guid id);
    Task<Result> Create(OrderCreateRequestDto request);
    Task<Result> Delete(Guid id);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<OrderListResponseDto>>> GetList(OrderListRequestDto request)
    {
        try
        {
            var query = _db.Orders.AsNoTracking().AsQueryable();

            if (request.BranchId.HasValue) query = query.Where(o => o.BranchId == request.BranchId.Value);

            if (request.ProcessedById.HasValue)
                query = query.Where(o => o.ProcessedById == request.ProcessedById.Value);

            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(take)
                .OrderByDescending(order => order.CreatedAt)
                .Select(order => new OrderListResponseDto
                {
                    Id = order.Id,
                    BranchId = order.BranchId,
                    ProcessedById = order.ProcessedById,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount
                })
                .ToListAsync();

            var result = new PagedResult<OrderListResponseDto>(items, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<OrderListResponseDto>>.Success(result);
        }
        catch (Exception e)
        {
            return Result<PagedResult<OrderListResponseDto>>.Failure(new InternalError("Order.GetList", e.Message));
        }
    }

    public async Task<Result<OrderGetByIdResponseDto>> GetById(Guid id)
    {
        const string errCode = "Order.GetById";
        try
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return Result<OrderGetByIdResponseDto>.Failure(new NotFoundError(errCode, "Order does not exist"));

            var dto = new OrderGetByIdResponseDto
            {
                Id = order.Id,
                BranchId = order.BranchId,
                ProcessedById = order.ProcessedById,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    SubTotal = oi.SubTotal
                }).ToList()
            };

            return Result<OrderGetByIdResponseDto>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<OrderGetByIdResponseDto>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Create(OrderCreateRequestDto request)
    {
        const string errCode = "Order.Create";
        try
        {
            var branchExists = await _db.Branches.AnyAsync(b => b.Id == request.BranchId);
            if (!branchExists)
                return Result.Failure(new NotFoundError(errCode, "Branch does not exist"));

            var userExists = await _db.Users.AnyAsync(u => u.Id == request.ProcessedById);
            if (!userExists)
                return Result.Failure(new NotFoundError(errCode, "User does not exist"));

            var productIds = request.OrderedItems.Select(item => item.ProductId).ToList();
            var inventories = await _db.BranchInventories.Where(bi => productIds.Contains(bi.ProductId))
                .Include("Product")
                .ToListAsync();
            if (inventories.Count() != productIds.Count())
                return Result.Failure(new NotFoundError(errCode, "One or more products do not exist"));

            var orderItems = new List<OrderItem>();
            decimal totalOrderAmount = 0;

            foreach (var orderedItem in request.OrderedItems)
            {
                var inventory = inventories.First(p => p.ProductId == orderedItem.ProductId);
                var product = inventory.Product;
                if (inventory.StockQuantity < orderedItem.Quantity)
                    return Result.Failure(new BadRequestError(errCode, $"Insufficient stock for: {product.Name}"));

                inventory.StockQuantity -= orderedItem.Quantity;
                var calculatedSubTotal = orderedItem.Quantity * product.Price;
                totalOrderAmount += calculatedSubTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = orderedItem.Quantity,
                    UnitPrice = product.Price,
                    SubTotal = calculatedSubTotal
                });
            }

            var order = new Order
            {
                BranchId = request.BranchId,
                ProcessedById = request.ProcessedById,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalOrderAmount,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            await _db.Orders.AddAsync(order);
            var result = await _db.SaveChangesAsync();
            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError(errCode, "Failed to create order"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Delete(Guid id)
    {
        const string errCode = "Order.Delete";
        try
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return Result.Failure(new NotFoundError(errCode, "Order does not exist"));

            order.DeletedAt = DateTime.UtcNow;
            var result = await _db.SaveChangesAsync();

            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError(errCode, "Failed to delete order"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }
}

public class OrderListRequestDto : PaginationFilter
{
    public Guid? BranchId { get; set; }
    public Guid? ProcessedById { get; set; }
}

public class OrderListResponseDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ProcessedById { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderGetByIdResponseDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ProcessedById { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemResponseDto> OrderItems { get; set; } = new();
}

public class OrderItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}

public class OrderCreateRequestDto
{
    public Guid BranchId { get; set; }
    public Guid ProcessedById { get; set; }
    public List<OrderItemCreateRequestDto> OrderedItems { get; set; } = new();
}

public class OrderItemCreateRequestDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}