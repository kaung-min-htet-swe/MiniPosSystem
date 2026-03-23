using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace MiniPos.Backend.Features.Products;

public interface IProductService
{
    Task<Result<PagedResult<ProductListResponseDto>>> GetList(ProductListRequestDto request);
    Task<Result<ProductGetByIdResponseDto>> GetById(Guid id);
    Task<Result<ProductCreateResponseDto>> Create(ProductCreateRequestDto request);
    Task<Result> Update(Guid id, ProductUpdateRequestDto request);
    Task<Result> Delete(Guid id);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<ProductListResponseDto>>> GetList(ProductListRequestDto request)
    {
        try
        {
            var query = _db.Products.AsNoTracking().AsQueryable();

            if (request.MerchantId.HasValue)
                query = query.Where(x => x.MerchantId == request.MerchantId.Value);

            if (request.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == request.CategoryId.Value);

            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.Where(x => x.Name.Contains(request.SearchTerm) || x.Sku.Contains(request.SearchTerm));

            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;
            var totalCount = await query.CountAsync();
            
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(p => new ProductListResponseDto
                {
                    Id = p.Id,
                    MerchantId = p.MerchantId,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    Sku = p.Sku,
                    Price = p.Price,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            var result = new PagedResult<ProductListResponseDto>(products, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<ProductListResponseDto>>.Success(result);
        }
        catch (Exception e)
        {
            return Result<PagedResult<ProductListResponseDto>>.Failure(new InternalError("Product.GetList", e.Message));
        }
    }

    public async Task<Result<ProductGetByIdResponseDto>> GetById(Guid id)
    {
        try
        {
            var product = await _db.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductGetByIdResponseDto
                {
                    Id = p.Id,
                    MerchantId = p.MerchantId,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    Sku = p.Sku,
                    Price = p.Price,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (product is null)
                return Result<ProductGetByIdResponseDto>.Failure(new NotFoundError("Product.GetById", "Product not found"));

            return Result<ProductGetByIdResponseDto>.Success(product);
        }
        catch (Exception e)
        {
            return Result<ProductGetByIdResponseDto>.Failure(new InternalError("Product.GetById", e.Message));
        }
    }

    public async Task<Result<ProductCreateResponseDto>> Create(ProductCreateRequestDto request)
    {
        try
        {
            var merchantExists = await _db.Merchants.AnyAsync(m => m.Id == request.MerchantId && m.IsActive);
            if (!merchantExists)
                return Result<ProductCreateResponseDto>.Failure(new NotFoundError("Product.Create", "Merchant not found or inactive"));

            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
                return Result<ProductCreateResponseDto>.Failure(new NotFoundError("Product.Create", "Category not found"));

            var isSkuExists = await _db.Products.AnyAsync(p => p.Sku == request.Sku && p.MerchantId == request.MerchantId);
            if (isSkuExists)
                return Result<ProductCreateResponseDto>.Failure(new ConflictError("Product.Create", "Product with this SKU already exists for this merchant"));

            var product = new Product
            {
                Id = Guid.NewGuid(),
                MerchantId = request.MerchantId,
                CategoryId = request.CategoryId,
                Name = request.Name,
                Sku = request.Sku,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Products.AddAsync(product);
            var result = await _db.SaveChangesAsync();

            return result > 0 
                ? Result<ProductCreateResponseDto>.Success(new ProductCreateResponseDto { Id = product.Id })
                : Result<ProductCreateResponseDto>.Failure(new InternalError("Product.Create", "Failed to create product"));
        }
        catch (Exception e)
        {
            return Result<ProductCreateResponseDto>.Failure(new InternalError("Product.Create", e.Message));
        }
    }

    public async Task<Result> Update(Guid id, ProductUpdateRequestDto request)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return Result.Failure(new NotFoundError("Product.Update", "Product not found"));

            if (request.CategoryId.HasValue)
            {
                var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value);
                if (!categoryExists)
                    return Result.Failure(new NotFoundError("Product.Update", "Category not found"));
                product.CategoryId = request.CategoryId.Value;
            }

            if (!string.IsNullOrEmpty(request.Sku) && request.Sku != product.Sku)
            {
                var isSkuExists = await _db.Products.AnyAsync(p => p.Sku == request.Sku && p.MerchantId == product.MerchantId && p.Id != id);
                if (isSkuExists)
                    return Result.Failure(new ConflictError("Product.Update", "Product with this SKU already exists for this merchant"));
                product.Sku = request.Sku;
            }

            if (!string.IsNullOrEmpty(request.Name))
                product.Name = request.Name;

            if (request.Price.HasValue)
                product.Price = request.Price.Value;

            product.UpdatedAt = DateTime.UtcNow;

            var result = await _db.SaveChangesAsync();
            return result > 0 
                ? Result.Success()
                : Result.Failure(new InternalError("Product.Update", "Failed to update product"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError("Product.Update", e.Message));
        }
    }

    public async Task<Result> Delete(Guid id)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return Result.Failure(new NotFoundError("Product.Delete", "Product not found"));

            product.DeletedAt = DateTime.UtcNow;
            var result = await _db.SaveChangesAsync();

            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError("Product.Delete", "Failed to delete product"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError("Product.Delete", e.Message));
        }
    }
}

public class ProductListRequestDto : PaginationFilter
{
    public Guid? MerchantId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? SearchTerm { get; set; }
}

public class ProductListResponseDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductGetByIdResponseDto : ProductListResponseDto
{
}

public class ProductCreateRequestDto
{
    public Guid MerchantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
}

public class ProductCreateResponseDto
{
    public Guid Id { get; set; }
}

public class ProductUpdateRequestDto
{
    public Guid? CategoryId { get; set; }
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
}