using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace MiniPos.Backend.Features.Branches;

public interface IBranchService
{
    Task<Result<PagedResult<BranchListResponseDto>>> GetList(BranchListRequestDto request);
    Task<Result<BranchGetByIdResponseDto>> GetById(Guid id);
    Task<Result> Create(BranchCreateRequestDto request);
    Task<Result> Update(Guid id, BranchUpdateRequestDto request);
    Task<Result> Delete(Guid id);
}

public class BranchService : IBranchService
{
    private readonly AppDbContext _db;

    public BranchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<BranchListResponseDto>>> GetList(BranchListRequestDto request)
    {
        try
        {
            var query = _db.Branches.AsNoTracking().AsQueryable();

            if (request.MerchantId.HasValue) query = query.Where(b => b.MerchantId == request.MerchantId.Value);

            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(take)
                .Include(b => b.Merchant)
                .OrderByDescending(branch => branch.CreatedAt)
                .Select(b => new BranchListResponseDto
                {
                    Id = b.Id,
                    MerchantId = b.MerchantId,
                    Name = b.Name,
                    Address = b.Address
                })
                .ToListAsync();

            var result =
                new PagedResult<BranchListResponseDto>(items, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<BranchListResponseDto>>.Success(result);
        }
        catch (Exception e)
        {
            return Result<PagedResult<BranchListResponseDto>>.Failure(new InternalError("Branch.GetList", e.Message));
        }
    }

    public async Task<Result<BranchGetByIdResponseDto>> GetById(Guid id)
    {
        const string errCode = "Branch.GetById";
        try
        {
            var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == id);
            if (branch == null)
                return Result<BranchGetByIdResponseDto>.Failure(new NotFoundError(errCode, "Branch does not exist"));

            var dto = new BranchGetByIdResponseDto
            {
                Id = branch.Id,
                MerchantId = branch.MerchantId,
                Name = branch.Name,
                Address = branch.Address
            };

            return Result<BranchGetByIdResponseDto>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<BranchGetByIdResponseDto>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Create(BranchCreateRequestDto request)
    {
        const string errCode = "Branch.Create";
        try
        {
            var isMerchantExist = await _db.Merchants.AnyAsync(m => m.Id == request.MerchantId);
            if (!isMerchantExist)
                return Result.Failure(new NotFoundError(errCode, "Merchant does not exist"));

            var isExist =
                await _db.Branches.AnyAsync(b => b.Name == request.Name && b.MerchantId == request.MerchantId);
            if (isExist)
                return Result.Failure(new ConflictError(errCode, "Branch already exist for this merchant"));

            var branch = new Branch
            {
                MerchantId = request.MerchantId,
                Name = request.Name,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Branches.AddAsync(branch);
            var result = await _db.SaveChangesAsync();
            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError("Branch.Create", "Failed to create branch"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Update(Guid id, BranchUpdateRequestDto request)
    {
        const string errCode = "Branch.Update";
        try
        {
            var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == id);
            if (branch == null)
                return Result.Failure(new NotFoundError(errCode, "Branch does not exist"));

            if (branch.MerchantId != request.MerchantId)
            {
                var isMerchantExist =
                    await _db.Merchants.AnyAsync(m => m.Id == request.MerchantId && m.DeletedAt == null);
                if (!isMerchantExist)
                    return Result.Failure(new NotFoundError(errCode, "Merchant does not exist"));
            }

            branch.MerchantId = request.MerchantId;
            branch.Name = request.Name;
            branch.Address = request.Address;
            branch.UpdatedAt = DateTime.UtcNow;

            var result = await _db.SaveChangesAsync();
            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError("Branch.Update", "Failed to update branch"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError("Branch.Update", e.Message));
        }
    }

    public async Task<Result> Delete(Guid id)
    {
        const string errCode = "Branch.Delete";
        try
        {
            var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
            if (branch == null)
                return Result.Failure(new NotFoundError(errCode, "Branch does not exist"));

            branch.DeletedAt = DateTime.UtcNow;
            var result = await _db.SaveChangesAsync();

            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError(errCode, "Failed to delete branch"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }
}

public class BranchListRequestDto : PaginationFilter
{
    public Guid? MerchantId { get; set; }
}

public class BranchListResponseDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}

public class BranchGetByIdResponseDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}

public class BranchCreateRequestDto
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}

public class BranchUpdateRequestDto
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}