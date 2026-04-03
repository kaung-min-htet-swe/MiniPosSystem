using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;
using MiniPos.Backend.Features.Users;

namespace MiniPos.Backend.Features.Merchants;

public interface IMerchantService
{
    Task<Result<PagedResult<MerchantListResponse>>> GetList(MerchantListRequest request);
    Task<Result<MerchantGetByIdResponse>> GetById(MerchantGetByIdRequest request);
    Task<Result<MerchantCreateResponse>> Create(MerchantCreateRequest request);
    Task<Result> Update(Guid id, MerchantUpdateRequest request);
    Task<Result> Delete(MerchantDeleteRequest request);
}

public class MerchantService : IMerchantService
{
    private readonly AppDbContext _db;

    public MerchantService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<MerchantListResponse>>> GetList(MerchantListRequest request)
    {
        try
        {
            var query = _db.Merchants
                .Where(m => m.Users.Any(u => u.Id == request.MerchantAdminId))
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(m => m.Name.Contains(request.SearchTerm));
            }

            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;
            var totalCount = await query.CountAsync();
            var merchants = await _db.Merchants
                .Skip(skip)
                .Take(take)
                .Select(m => new MerchantListResponse
                {
                    Id = m.Id,
                    Name = m.Name,
                    ContactEmail = m.ContactEmail,
                    BranchCount = m.Branches.Count,
                })
                .ToListAsync();

            var result =
                new PagedResult<MerchantListResponse>(merchants, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<MerchantListResponse>>.Success(result);
        }
        catch (Exception e)
        {
            return Result<PagedResult<MerchantListResponse>>.Failure(new InternalError("Merchant.GetList", e.Message));
        }
    }

    public async Task<Result<MerchantGetByIdResponse>> GetById(MerchantGetByIdRequest request)
    {
        try
        {
            var merchant = await _db.Merchants
                .AsNoTracking()
                .Where(m => m.Id == request.MerchantId && m.Users.Any(u => u.Id == request.MerchantAdminId))
                .Select(m => new MerchantGetByIdResponse
                {
                    Id = m.Id,
                    Name = m.Name,
                    ContactEmail = m.ContactEmail,
                    IsActive = m.IsActive,
                    CreatedAt = m.CreatedAt,
                    BranchCount = m.Branches.Count,
                    EmployeeCount = m.Users.Count(u => u.Role == nameof(UserRole.Cashier)),
                    ProductCount = m.Products.Count,
                })
                .FirstOrDefaultAsync();

            if (merchant == null)
                return Result<MerchantGetByIdResponse>.Failure(new NotFoundError("Merchant.GetById",
                    "Merchant not found"));

            return Result<MerchantGetByIdResponse>.Success(merchant);
        }
        catch (Exception e)
        {
            return Result<MerchantGetByIdResponse>.Failure(new InternalError("Merchant.GetById", e.Message));
        }
    }

    public async Task<Result<MerchantCreateResponse>> Create(MerchantCreateRequest request)
    {
        try
        {
            var merchant = new Merchant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ContactEmail = request.ContactEmail,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Merchants.AddAsync(merchant);
            await _db.SaveChangesAsync();

            return Result<MerchantCreateResponse>.Success(new MerchantCreateResponse { Id = merchant.Id });
        }
        catch (Exception e)
        {
            return Result<MerchantCreateResponse>.Failure(new InternalError("Merchant.Create", e.Message));
        }
    }

    public async Task<Result> Update(Guid id, MerchantUpdateRequest request)
    {
        try
        {
            var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null);
            if (merchant == null)
                return Result.Failure(new NotFoundError("Merchant.Update", "Merchant not found"));

            merchant.Name = request.Name;
            merchant.ContactEmail = request.ContactEmail;
            merchant.IsActive = request.IsActive;
            merchant.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError("Merchant.Update", e.Message));
        }
    }

    public async Task<Result> Delete(MerchantDeleteRequest request)
    {
        try
        {
            var merchant = await _db.Merchants
                .FirstOrDefaultAsync(m => m.Id == request.MerchantId &&
                                          m.Users.Any(u => u.Id == request.MerchantAdminId));
            if (merchant == null)
                return Result.Failure(new NotFoundError("Merchant.Delete", "Merchant not found"));

            merchant.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError("Merchant.Delete", e.Message));
        }
    }
}

public class MerchantListRequest : PaginationFilter
{
    public Guid MerchantAdminId { get; set; }
    public string? SearchTerm { get; set; }
}

public class MerchantListResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? ContactEmail { get; set; }
    public int BranchCount { get; set; }
}

public class MerchantGetByIdRequest
{
    public Guid MerchantAdminId { get; set; }
    public Guid MerchantId { get; set; }
}

public class MerchantGetByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int BranchCount { get; set; }
    public int ProductCount { get; set; }
    public int EmployeeCount { get; set; }
}

public class MerchantCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public Guid MerchantAdminId { get; set; }
}

public class MerchantCreateResponse
{
    public Guid Id { get; set; }
}

public class MerchantUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public Guid MerchantAdminId { get; set; }
}

public class MerchantDeleteRequest
{
    public Guid MerchantAdminId { get; set; }
    public Guid MerchantId { get; set; }
}