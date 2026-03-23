using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace MiniPos.Backend.Features.BranchInventories;

public interface IBranchInventoryService
{
    Task<Result<PagedResult<BranchInventoryListResponseDto>>> GetList(BranchInventoryListRequestDto request);
    Task<Result<BranchInventoryGetByIdResponseDto>> GetById(Guid id);
    Task<Result> Create(BranchInventoryCreateRequestDto request);
    Task<Result> Update(Guid id, BranchInventoryUpdateRequestDto request);
    Task<Result> Delete(Guid id);
}

public class BranchInventoryService : IBranchInventoryService
{
    private readonly AppDbContext _db;

    public BranchInventoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<BranchInventoryListResponseDto>>> GetList(
        BranchInventoryListRequestDto request)
    {
        try
        {
            var isMerchantExists = await _db.Merchants.AnyAsync(merchant => merchant.Id == request.MerchantId);
            if (!isMerchantExists)
                return Result<PagedResult<BranchInventoryListResponseDto>>.Failure(new NotFoundError("Product.GetList",
                    "Merchant does not exist"));

            var isBranchExists = await _db.Branches.AnyAsync(branch => branch.Id == request.BranchId);
            if (!isBranchExists)
                return Result<PagedResult<BranchInventoryListResponseDto>>.Failure(new NotFoundError("Product.GetList",
                    "Branch does not exist"));

            var query = _db.BranchInventories
                .Where(inventory => inventory.BranchId == request.BranchId)
                .AsNoTracking()
                .AsQueryable();

            if (request.CategoryId.HasValue)
                query = query.Where(inventory => inventory.Product.Id == request.CategoryId);

            var totalCount = await query.CountAsync();
            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;
            var items = await query
                .Skip(skip)
                .Take(take)
                .OrderByDescending(product => product.CreatedAt)
                .Include(inventory => inventory.Product)
                .Select(inventory => new BranchInventoryListResponseDto
                {
                    Id = inventory.Id,
                    Name = inventory.Product.Name,
                    Sku = inventory.Product.Sku,
                    Price = inventory.Product.Price,
                    StockQuantity = inventory.StockQuantity
                })
                .ToListAsync();

            var result =
                new PagedResult<BranchInventoryListResponseDto>(items, totalCount, request.PageNumber,
                    request.PageSize);
            return Result<PagedResult<BranchInventoryListResponseDto>>.Success(result);
        }
        catch (Exception e)
        {
            return Result<PagedResult<BranchInventoryListResponseDto>>.Failure(new InternalError("Product.GetList",
                e.Message));
        }
    }

    public async Task<Result<BranchInventoryGetByIdResponseDto>> GetById(Guid id)
    {
        const string errCode = "Product.GetById";
        try
        {
            var products =
                await _db.BranchInventories
                    .AsNoTracking()
                    .Include(inventory => inventory.Product)
                    .Where(inventory => inventory.Id == id)
                    .Select(inventory => new BranchInventoryGetByIdResponseDto
                    {
                        Id = inventory.Id,
                        StockQuantity = inventory.StockQuantity,
                        Name = inventory.Product.Name,
                        Price = inventory.Product.Price,
                        Sku = inventory.Product.Sku
                    })
                    .ToListAsync();

            return products.Count == 0
                ? Result<BranchInventoryGetByIdResponseDto>.Failure(
                    new NotFoundError(errCode, "Product does not exist"))
                : Result<BranchInventoryGetByIdResponseDto>.Success(products[0]);
        }
        catch (Exception e)
        {
            return Result<BranchInventoryGetByIdResponseDto>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Create(BranchInventoryCreateRequestDto request)
    {
        const string errCode = "Product.Create";
        try
        {
            var merchantExists = await _db.Merchants.AnyAsync(m => m.Id == request.MerchantId);
            if (!merchantExists)
                return Result.Failure(new NotFoundError(errCode, "Merchant does not exist"));

            var categoryExists =
                await _db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.MerchantId == request.MerchantId);
            if (!categoryExists)
                return Result.Failure(new NotFoundError(errCode, "Category does not exist for this merchant"));

            var productExist =
                await _db.Products.AnyAsync(p => p.Sku == request.Sku && p.MerchantId == request.MerchantId);
            if (productExist)
                return Result.Failure(new ConflictError(errCode,
                    "Product with the same SKU already exists for this merchant"));

            var branch = await _db
                .Branches
                .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.MerchantId == request.MerchantId);
            if (branch == null)
                return Result.Failure(new NotFoundError(errCode, "Branch does not exist for this merchant"));

            var product = new Product
            {
                MerchantId = request.MerchantId,
                CategoryId = request.CategoryId,
                Name = request.Name,
                Sku = request.Sku,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Products.AddAsync(product);
            var result = await _db.SaveChangesAsync();
            if (result == 0)
                return Result.Failure(new InternalError(errCode, "Failed to create product"));

            var branchInventory = new BranchInventory
            {
                BranchId = request.BranchId,
                ProductId = product.Id,
                StockQuantity = request.StockQuantity
            };

            await _db.BranchInventories.AddAsync(branchInventory);
            result = await _db.SaveChangesAsync();
            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError("BranchInventory.Create", "Failed to create branch inventory"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Update(Guid id, BranchInventoryUpdateRequestDto request)
    {
        const string errCode = "Product.Update";
        try
        {
            var inventory = await _db.BranchInventories
                .Include(inventory => inventory.Product)
                .FirstOrDefaultAsync(inventory => inventory.Id == id);
            if (inventory == null)
                return Result.Failure(new NotFoundError(errCode, "Product does not exist"));

            if (request.Price.HasValue)
            {
                inventory.Product.Price = request.Price.Value;
                inventory.Product.UpdatedAt = DateTime.UtcNow;
            }

            if (request.StockQuantity.HasValue)
            {
                inventory.StockQuantity = request.StockQuantity.Value;
                inventory.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Delete(Guid id)
    {
        const string errCode = "Product.Delete";
        try
        {
            var product = await _db.BranchInventories.FirstOrDefaultAsync(inventory => inventory.Id == id);
            if (product == null)
                return Result.Failure(new NotFoundError(errCode, "Product does not exist"));

            product.DeletedAt = DateTime.UtcNow;
            var result = await _db.SaveChangesAsync();

            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError(errCode, "Failed to delete product"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }
}

public class BranchInventoryListRequestDto : PaginationFilter
{
    public Guid MerchantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? CategoryId { get; set; }
}

public class BranchInventoryListResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? CategoryName { get; set; }
}

public class BranchInventoryGetByIdResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? CategoryName { get; set; }
}

public class BranchInventoryCreateRequestDto
{
    public Guid MerchantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class BranchInventoryUpdateRequestDto
{
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
}