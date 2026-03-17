using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace MiniPos.Backend.Features.Categories;

public interface ICategoryService
{
    Task<Result<PagedResult<CategoryListResponseDto>>> GetList(CategoryListRequestDto request);
    Task<Result<CategoryGetByIdResponseDto>> GetById(Guid id);
    Task<Result> Create(CategoryCreateRequestDto request);
    Task<Result> Update(Guid id, CategoryUpdateRequestDto request);
    Task<Result> Delete(Guid id);
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<CategoryListResponseDto>>> GetList(CategoryListRequestDto request)
    {
        try
        {
            var query = _db.Categories.AsNoTracking().AsQueryable();

            if (request.MerchantId.HasValue)
            {
                query = query.Where(c => c.MerchantId == request.MerchantId.Value);
            }

            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(take)
                .OrderByDescending(category => category.CreatedAt)
                .Select(category => new CategoryListResponseDto
                {
                    Id = category.Id,
                    MerchantId = category.MerchantId,
                    Name = category.Name,
                    Description = category.Description,
                })
                .ToListAsync();

            var result =
                new PagedResult<CategoryListResponseDto>(items, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<CategoryListResponseDto>>.Success(result);
        }
        catch (Exception e)
        {
            return Result<PagedResult<CategoryListResponseDto>>.Failure(
                new InternalError("Category.GetList", e.Message));
        }
    }

    public async Task<Result<CategoryGetByIdResponseDto>> GetById(Guid id)
    {
        const string errCode = "Category.GetById";
        try
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return Result<CategoryGetByIdResponseDto>.Failure(new NotFoundError(errCode,
                    "Category does not exist"));

            var dto = new CategoryGetByIdResponseDto
            {
                Id = category.Id,
                MerchantId = category.MerchantId,
                Name = category.Name,
                Description = category.Description,
            };

            return Result<CategoryGetByIdResponseDto>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<CategoryGetByIdResponseDto>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Create(CategoryCreateRequestDto request)
    {
        const string errCode = "Category.Create";
        try
        {
            var merchantExists = await _db.Merchants.AnyAsync(m => m.Id == request.MerchantId);
            if (!merchantExists)
                return Result.Failure(new NotFoundError(errCode, "Merchant does not exist"));

            var isExist =
                await _db.Categories.AnyAsync(c => c.Name == request.Name && c.MerchantId == request.MerchantId);
            if (isExist)
                return Result.Failure(new ConflictError(errCode,
                    "Category with the same name already exists for this merchant"));

            var category = new Category
            {
                MerchantId = request.MerchantId,
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Categories.AddAsync(category);
            var result = await _db.SaveChangesAsync();
            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError(errCode, "Failed to create category"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Update(Guid id, CategoryUpdateRequestDto request)
    {
        const string errCode = "Category.Update";
        try
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return Result.Failure(new NotFoundError(errCode, "Category does not exist"));

            if (category.MerchantId != request.MerchantId)
            {
                var merchantExists = await _db.Merchants.AnyAsync(m => m.Id == request.MerchantId);
                if (!merchantExists)
                    return Result.Failure(new NotFoundError(errCode, "Merchant does not exist"));
            }

            category.MerchantId = request.MerchantId;
            category.Name = request.Name;
            category.Description = request.Description;
            category.UpdatedAt = DateTime.UtcNow;

            var result = await _db.SaveChangesAsync();
            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError(errCode, "Failed to update category"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Delete(Guid id)
    {
        const string errCode = "Category.Delete";
        try
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return Result.Failure(new NotFoundError(errCode, "Category does not exist"));

            category.DeletedAt = DateTime.UtcNow;
            var result = await _db.SaveChangesAsync();

            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError(errCode, "Failed to delete category"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }
}

public class CategoryListRequestDto : PaginationFilter
{
    public Guid? MerchantId { get; set; }
}

public class CategoryListResponseDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CategoryGetByIdResponseDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CategoryCreateRequestDto
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CategoryUpdateRequestDto
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}