using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace MiniPos.Backend.Features.Merchants;

public interface IMerchantService
{
    Task<Result<PagedResult<MerchantListResponseDto>>> GetList(MerchantListRequestDto request);
    Task<Result<MerchantGetByIdResponseDto>> GetById(Guid id);
    Task<Result<MerchantCreateResponseDto>> Create(MerchantCreateRequestDto request);
    Task<Result> Update(Guid id, MerchantUpdateRequestDto request);
    Task<Result> Delete(Guid id);
}

public class MerchantService : IMerchantService
{
    private readonly AppDbContext _db;

    public MerchantService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<MerchantListResponseDto>>> GetList(MerchantListRequestDto request)
    {
        try
        {
            var query = _db.Merchants
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(m => m.Name.Contains(request.SearchTerm));
            }

            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;
            var totalCount = await query.CountAsync();
            var merchantAdminId = Guid.Parse(request.MerchantAdminId!);
            var merchants = await _db.Merchants
                .AsNoTracking()
                .Skip(skip)
                .Take(take)
                .Where(m => m.Users.Any(mu => 
                    mu.Id == merchantAdminId && 
                    mu.Role == "Merchant"))
                .ToListAsync();
            
            var dto = merchants.Select(m => new MerchantListResponseDto {
                Id = m.Id,
                Name = m.Name,
                ContactEmail = m.ContactEmail,
            }).ToList();

            var result = new PagedResult<MerchantListResponseDto>(dto, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<MerchantListResponseDto>>.Success(result);
        }
        catch (Exception e)
        {
            return Result<PagedResult<MerchantListResponseDto>>.Failure(new InternalError("Merchant.GetList", e.Message));
        }
    }

    public async Task<Result<MerchantGetByIdResponseDto>> GetById(Guid id)
    {
        try
        {
            var merchant = await _db.Merchants
                .AsNoTracking()
                .Where(m => m.Id == id && m.DeletedAt == null)
                .Select(m => new MerchantGetByIdResponseDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ContactEmail = m.ContactEmail,
                    IsActive = m.IsActive,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (merchant == null)
                return Result<MerchantGetByIdResponseDto>.Failure(new NotFoundError("Merchant.GetById", "Merchant not found"));

            return Result<MerchantGetByIdResponseDto>.Success(merchant);
        }
        catch (Exception e)
        {
            return Result<MerchantGetByIdResponseDto>.Failure(new InternalError("Merchant.GetById", e.Message));
        }
    }

    public async Task<Result<MerchantCreateResponseDto>> Create(MerchantCreateRequestDto request)
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

            return Result<MerchantCreateResponseDto>.Success(new MerchantCreateResponseDto { Id = merchant.Id });
        }
        catch (Exception e)
        {
            return Result<MerchantCreateResponseDto>.Failure(new InternalError("Merchant.Create", e.Message));
        }
    }

    public async Task<Result> Update(Guid id, MerchantUpdateRequestDto request)
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

    public async Task<Result> Delete(Guid id)
    {
        try
        {
            var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null);
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

public class MerchantListRequestDto : PaginationFilter
{
    public string? MerchantAdminId { get; set; }
    public string? SearchTerm { get; set; }
}

public class MerchantListResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
}

public class MerchantGetByIdResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MerchantCreateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
}

public class MerchantCreateResponseDto
{
    public Guid Id { get; set; }
}

public class MerchantUpdateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
}
