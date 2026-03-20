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
    Task<Result> Delete(Guid id);
}

public enum UserRole
{
    Admin,
    Merchant,
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
            var query = _db.Users.AsNoTracking().AsQueryable();

            var skip = (request.PageNumber - 1) * request.PageSize;
            var take = request.PageSize;
            var totalCount = await query.CountAsync();
            var users = await query
                .Skip(skip)
                .Take(take)
                .OrderByDescending(user => user.CreatedAt)
                .ToListAsync();
            var userDtos = users.Select(user => new UserListResponseDto
            {
                Id = user.Id,
                UserName = user.Username,
                Email = user.Email,
                Role = user.Role
            }).ToList();

            var result =
                new PagedResult<UserListResponseDto>(userDtos, totalCount, request.PageNumber, request.PageSize);
            return Result<PagedResult<UserListResponseDto>>.Success(result);
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
                nameof(UserRole.Merchant) => await GetMerchantById(request.Id),
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
                nameof(UserRole.Merchant) => await CreateMerchant(request),
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

        var merchant = new Merchant
        {
            Name = merchantDto.Name,
            ContactEmail = merchantDto.ContactEmail,
            IsActive = true
        };
        await _db.Merchants.AddAsync(merchant);
        if (await _db.SaveChangesAsync() == 0)
            return Result<UserCreateResponseDto>.Failure(new InternalError(errCode,
                "Failed to create merchant for user"));

        var user = new User
        {
            Email = request.Email,
            Role = nameof(UserRole.Merchant),
            Username = request.UserName,
            MerchantId = merchant.Id
        };
        var passwordHash = _passwordHasher.HashPassword(user, request.Password);
        user.PasswordHash = passwordHash;

        await _db.Users.AddAsync(user);
        var result = await _db.SaveChangesAsync();
        return result > 0
            ? Result<UserCreateResponseDto>.Success(new UserCreateResponseDto { Id = user.Id })
            : Result<UserCreateResponseDto>.Failure(new InternalError("User.Create", "Failed to create user"));
    }

    private async Task<Result<UserCreateResponseDto>> CreateCashier(UserCreateRequestDto request)
    {
        const string errCode = "User.CreateCashier";
        if (request.BranchId is null || request.ProcessedById is null)
            return Result<UserCreateResponseDto>.Failure(new BadRequestError(errCode,
                "Processor and Branch is required to create a new cashier."));

        var branchId = Guid.Parse(request.BranchId);
        var isBranchExist = await _db.Branches.AnyAsync(b => b.Id == branchId);
        if (!isBranchExist)
            return Result<UserCreateResponseDto>.Failure(new NotFoundError(errCode, "Branch does not exist"));

        var merchantAdminId = Guid.Parse(request.ProcessedById);
        var isAdminExist = await _db.Users.AnyAsync(u => u.BranchId == branchId && u.Role == nameof(UserRole.Merchant));
        if (!isAdminExist)
            return Result<UserCreateResponseDto>.Failure(new NotFoundError(errCode,
                "Merchant admin does not exist with this branch"));

        var user = new User
        {
            Email = request.Email,
            Role = nameof(UserRole.Cashier),
            Username = request.UserName,
            BranchId = branchId
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
            .Include(user => user.Merchant)
            .Select(u => new UserGetByIdResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                UserName = u.Username,
                Merchant = new MerchantDto
                {
                    Id = u.Merchant.Id,
                    Name = u.Merchant.Name,
                    ContactEmail = u.Merchant.ContactEmail
                }
            })
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == nameof(UserRole.Merchant));
        if (merchantAdmin is null)
            return Result<UserGetByIdResponseDto>.Failure(new NotFoundError("User.GetMerchantById",
                "Merchant admin does not exist"));

        return Result<UserGetByIdResponseDto>.Success(merchantAdmin);
    }

    private async Task<Result<UserGetByIdResponseDto>> GetCashierById(Guid id)
    {
        var cashier = await _db.Users
            .Include(user => user.Branch)
            .Select(u => new UserGetByIdResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                UserName = u.Username,
                Branch = new BranchDto
                {
                    Id = u.Branch.Id,
                    Name = u.Branch.Name,
                    Address = u.Branch.Address
                }
            })
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == nameof(UserRole.Cashier));
        if (cashier is null)
            return Result<UserGetByIdResponseDto>.Failure(new NotFoundError("User.GetCashierById",
                "Cashier does not exist"));

        return Result<UserGetByIdResponseDto>.Success(cashier);
    }
}

public class UserListRequestDto : PaginationFilter
{
}

public class UserListResponseDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}

public class MerchantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? ContactEmail { get; set; }
}

public class UserGetByIdRequestDto
{
    public Guid Id { get; set; }
    public string Role { get; set; }
}

public class BranchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}

public class UserGetByIdResponseDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public MerchantDto? Merchant { get; set; }
    public BranchDto? Branch { get; set; }
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