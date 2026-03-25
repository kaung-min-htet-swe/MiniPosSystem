using Common;
using Database.EfAppDbContextModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MiniPos.Backend.Features.Users;

public interface IUserService
{
    Task<Result<PagedResult<UserListResponseDto>>> GetList(UserListRequestDto request);
    Task<Result<UserGetByIdResponseDto>> GetById(UserGetByIdRequestDto request);
    Task<Result<UserCreateResponseDto>> Create(UserCreateRequestDto request);
    Task<Result> Update(Guid id, UserUpdateRequestDto request);
    Task<Result> ResetPassword(Guid id, UserUpdatePasswordRequestDto request);
    Task<Result> AssignBranch(Guid id, Guid branchId);
    Task<Result> Delete(Guid id);
}

public enum UserRole
{
    Admin,
    MerchantAdmin,
    Cashier
}

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<PagedResult<UserListResponseDto>>> GetList(UserListRequestDto request)
    {
        try
        {
            if (request.IncludeCashierRole)
            {
                var query = _db.Users
                    .Where(u => u.Role == nameof(UserRole.Cashier))
                    .AsNoTracking()
                    .AsQueryable();
                var skip = (request.PageNumber - 1) * request.PageSize;
                var take = request.PageSize;
                var totalCount = await query.CountAsync();

                var users = await query
                    .Skip(skip)
                    .Take(take)
                    .Select(u => new UserListResponseDto
                    {
                        Id = u.Id,
                        UserName = u.Username,
                        Email = u.Email,
                        Role = u.Role,

                        Merchant = u.Merchant != null
                            ? new Merchant
                            {
                                Id = u.Merchant.Id,
                                Name = u.Merchant.Name
                            }
                            : null,

                        Branch = u.Branch != null
                            ? new Branch
                            {
                                Id = u.Branch.Id,
                                Name = u.Branch.Name
                            }
                            : null
                    })
                    .ToListAsync();

                var result =
                    new PagedResult<UserListResponseDto>(users, totalCount, request.PageNumber, request.PageSize);
                return Result<PagedResult<UserListResponseDto>>.Success(result);
            }

            return Result<PagedResult<UserListResponseDto>>.Failure(new UnAuthorized("User.List", "UnAuthorized."));
        }
        catch (Exception e)
        {
            return Result<PagedResult<UserListResponseDto>>.Failure(new InternalError("User.GetList", e.Message));
        }
    }
    
    public async Task<Result<UserGetByIdResponseDto>> GetById(UserGetByIdRequestDto request)
    {
        const string errCode = "User.GetById";
        try
        {
            return request.Role switch
            {
                nameof(UserRole.MerchantAdmin) => await GetMerchantById(request.Id),
                nameof(UserRole.Cashier) => await GetCashierById(request.Id),
                _ => Result<UserGetByIdResponseDto>.Failure(new BadRequestError(errCode, "Invalid user role specified"))
            };
        }
        catch (Exception e)
        {
            return Result<UserGetByIdResponseDto>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result<UserCreateResponseDto>> Create(UserCreateRequestDto request)
    {
        const string errCode = "User.Create";
        try
        {
            var isExist = await _db.Users.AnyAsync(user => user.Email == request.Email);
            if (isExist)
                return Result<UserCreateResponseDto>.Failure(new ConflictError(errCode, "User already exist"));

            return request.Role switch
            {
                nameof(UserRole.MerchantAdmin) => await CreateMerchant(request),
                nameof(UserRole.Cashier) => await CreateCashier(request),
                _ => Result<UserCreateResponseDto>.Failure(new BadRequestError(errCode, "Invalid user role specified"))
            };
        }
        catch (Exception e)
        {
            return Result<UserCreateResponseDto>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Update(Guid id, UserUpdateRequestDto request)
    {
        const string errCode = "User.Update";
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (user == null)
                return Result.Failure(new NotFoundError(errCode, "User does not exist"));

            user.Email = request.Email;
            user.Username = request.UserName;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _db.SaveChangesAsync();
            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError("User.Update", "Failed to update user"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError("User.Update", e.Message));
        }
    }

    public async Task<Result> ResetPassword(Guid id, UserUpdatePasswordRequestDto request)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (user == null)
                return Result.Failure(new NotFoundError("User.UpdatePassword", "User does not exist"));
            
            if (request.NewPassword != request.ConfirmPassword)
                return Result.Failure(new BadRequestError("User.UpdatePassword", "New password and confirm password do not match"));
            
            var hashedPassword = _passwordHasher.HashPassword(user, request.NewPassword);
            user.PasswordHash = hashedPassword;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError("User.UpdatePassword", e.Message));
        }
    }

    public async Task<Result> AssignBranch(Guid id, Guid branchId)
    {
        const string errCode = "User.ReassignBranch";
        try
        {
            var isBranchExist = await _db.Branches.AnyAsync(b => b.Id == branchId);
            if (!isBranchExist)
                return Result.Failure(new NotFoundError(errCode, "Branch does not exist"));

            var user = await _db.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (user == null)
                return Result.Failure(new NotFoundError(errCode, "User does not exist"));
            
            user.BranchId = branchId;
            user.UpdatedAt = DateTime.UtcNow;
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
        const string errCode = "User.Delete";
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (user == null)
                return Result.Failure(new NotFoundError(errCode, "User does not exist"));

            user.DeletedAt = DateTime.UtcNow;
            var result = await _db.SaveChangesAsync();

            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError(errCode, "Failed to delete user"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
        }
    }

    private async Task<Result<UserCreateResponseDto>> CreateMerchant(UserCreateRequestDto request)
    {
        const string errCode = "User.CreateMerchant";
        if (request.ProcessedById is null)
            return Result<UserCreateResponseDto>.Failure(new NotFoundError(errCode,
                "Processer is required to create a new merchant"));

        var adminId = Guid.Parse(request.ProcessedById);
        var isAdminExist = await _db.Users.AnyAsync(u => u.Id == adminId && u.Role == nameof(UserRole.Admin));
        if (!isAdminExist)
            return Result<UserCreateResponseDto>.Failure(new NotFoundError("User.Create", "Admin does not exist"));

        var merchantDto = request.Merchant;
        if (merchantDto == null)
            return Result<UserCreateResponseDto>.Failure(new BadRequestError("User.Create",
                "Merchant details are required for Merchant role"));

        var merchant = new Database.EfAppDbContextModels.Merchant
        {
            Name = merchantDto.Name,
            ContactEmail = merchantDto.ContactEmail,
            IsActive = true
        };
        await _db.Merchants.AddAsync(merchant);
        if (await _db.SaveChangesAsync() == 0)
            return Result<UserCreateResponseDto>.Failure(new InternalError(errCode,
                "Failed to create merchant for user"));

        var merchantAdmin = new User
        {
            Email = request.Email,
            Role = nameof(UserRole.MerchantAdmin),
            Username = request.UserName,
            MerchantId = merchant.Id
        };
        var passwordHash = _passwordHasher.HashPassword(merchantAdmin, request.Password);
        merchantAdmin.PasswordHash = passwordHash;

        await _db.Users.AddAsync(merchantAdmin);
        var result = await _db.SaveChangesAsync();
        return result > 0
            ? Result<UserCreateResponseDto>.Success(new UserCreateResponseDto { Id = merchantAdmin.Id })
            : Result<UserCreateResponseDto>.Failure(new InternalError("User.Create", "Failed to create user"));
    }

    private async Task<Result<UserCreateResponseDto>> CreateCashier(UserCreateRequestDto request)
    {
        const string errCode = "User.CreateCashier";
        if (request.BranchId is null || request.ProcessedById is null)
            return Result<UserCreateResponseDto>.Failure(new BadRequestError(errCode,
                "Processor and Branch is required to create a new cashier."));

        var merchantId = Guid.Parse(request.MerchantId!);
        var branchId = Guid.Parse(request.BranchId);
        var isBranchExist = await _db.Branches
            .AsNoTracking()
            .Select(b => new { b.Id, MerchantId = b.Merchant.Id })
            .AnyAsync(b => b.Id == branchId && b.MerchantId == merchantId);
        if (!isBranchExist)
            return Result<UserCreateResponseDto>.Failure(new NotFoundError(errCode,
                "Branch does not exist with this merchant"));

        var user = new User
        {
            Email = request.Email,
            Role = nameof(UserRole.Cashier),
            Username = request.UserName,
            BranchId = branchId,
            MerchantId = merchantId
        };
        var passwordHash = _passwordHasher.HashPassword(user, request.Password);
        user.PasswordHash = passwordHash;

        await _db.Users.AddAsync(user);
        var result = await _db.SaveChangesAsync();
        return result > 0
            ? Result<UserCreateResponseDto>.Success(new UserCreateResponseDto { Id = user.Id })
            : Result<UserCreateResponseDto>.Failure(new InternalError(errCode, "Failed to create cashier"));
    }

    private async Task<Result<UserGetByIdResponseDto>> GetMerchantById(Guid id)
    {
        var merchantAdmin = await _db.Users
            .AsNoTracking()
            .Select(u => new UserGetByIdResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                UserName = u.Username
            })
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == nameof(UserRole.MerchantAdmin));
        if (merchantAdmin is null)
            return Result<UserGetByIdResponseDto>.Failure(new NotFoundError("User.GetMerchantById",
                "Merchant admin does not exist"));

        return Result<UserGetByIdResponseDto>.Success(merchantAdmin);
    }

    private async Task<Result<UserGetByIdResponseDto>> GetCashierById(Guid id)
    {
        var cashier = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == id && u.DeletedAt == null)
            .Select(u => new UserGetByIdResponseDto
            {
                Id = u.Id,
                UserName = u.Username,
                Email = u.Email,
                Role = u.Role,
                JoinedDate = u.CreatedAt,
                Merchant = u.Merchant != null
                    ? new Merchant
                    {
                        Id = u.Merchant.Id,
                        Name = u.Merchant.Name
                    }
                    : null,
                Branch = u.Branch != null
                    ? new Branch
                    {
                        Id = u.Branch.Id,
                        Name = u.Branch.Name
                    }
                    : null,
                TotalSales = u.Orders
                    .Where(o => o.DeletedAt == null)
                    .Sum(o => (decimal?)o.TotalAmount ?? 0)
                    .ToString("N2")
            })
            .FirstOrDefaultAsync();

        return Result<UserGetByIdResponseDto>.Success(cashier);
    }
}

public class Merchant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? ContactEmail { get; set; }
}

public class Branch
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}

public class UserListRequestDto : PaginationFilter
{
    public bool IncludeCashierRole { get; set; } = false;
    public bool IncludeMerchantRole { get; set; } = false;
}

public class UserListResponseDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Merchant? Merchant { get; set; }
    public Branch? Branch { get; set; }
}

public class UserGetByIdRequestDto
{
    public Guid Id { get; set; }
    public string Role { get; set; }
}

public class UserGetByIdResponseDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string TotalSales { get; set; } = string.Empty;
    public Merchant? Merchant { get; set; }
    public Branch? Branch { get; set; }
    public DateTime? JoinedDate { get; set; }
}

public class MerchantCreateDto
{
    public string Name { get; set; }
    public string? ContactEmail { get; set; }
}

public class UserCreateRequestDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Password { get; set; }
    public MerchantCreateDto? Merchant { get; set; }
    public string? MerchantId { get; set; }
    public string? ProcessedById { get; set; }
    public string? BranchId { get; set; }
}

public class UserCreateResponseDto
{
    public Guid Id { get; set; }
}

public class UserUpdateRequestDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
}

public class UserUpdatePasswordRequestDto
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserAssignBranchRequestDto
{
    public Guid BranchId { get; set; }
}