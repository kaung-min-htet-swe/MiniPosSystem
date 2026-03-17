using Common;
using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace MiniPos.Backend.Features.Users;

public interface IUserService
{
    Task<Result<PagedResult<UserListResponseDto>>> GetList(UserListRequestDto request);
    Task<Result<UserGetByIdResponseDto>> GetById(Guid id);
    Task<Result> Create(UserCreateRequestDto request);
    Task<Result> Update(Guid id, UserUpdateRequestDto request);
    Task<Result> Delete(Guid id);
}

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
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

    public async Task<Result<UserGetByIdResponseDto>> GetById(Guid id)
    {
        const string errCode = "User.GetById";
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (user == null)
                return Result<UserGetByIdResponseDto>.Failure(new NotFoundError(errCode, "User does not exist"));

            var userDto = new UserGetByIdResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                UserName = user.Username
            };

            return Result<UserGetByIdResponseDto>.Success(userDto);
        }
        catch (Exception e)
        {
            return Result<UserGetByIdResponseDto>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public async Task<Result> Create(UserCreateRequestDto request)
    {
        const string errCode = "User.Create";
        try
        {
            var isExist = await _db.Users.AnyAsync(user => user.Email == request.Email);
            if (isExist)
                return Result.Failure(new ConflictError(errCode, "User already exist"));

            var passwordHash = request.Password;
            var user = new User
            {
                Email = request.Email,
                Role = request.Role,
                Username = request.UserName,
                PasswordHash = passwordHash
            };

            switch (request.Role)
            {
                case "Merchant" when request.Merchant == null:
                    return Result.Failure(new BadRequestError(errCode,
                        "Merchant details are required for Merchant role"));
                case "Merchant":
                {
                    var merchantDto = request.Merchant;
                    var merchant = new Merchant
                    {
                        Name = merchantDto.Name,
                        ContactEmail = merchantDto.ContactEmail,
                        IsActive = true
                    };
                    await _db.Merchants.AddAsync(merchant);
                    if (await _db.SaveChangesAsync() == 0)
                        return Result.Failure(new InternalError(errCode, "Failed to create merchant for user"));

                    user.MerchantId = merchant.Id;
                    break;
                }

                case "Cashier" when request.MerchantId == null:
                    return Result.Failure(new BadRequestError(errCode, "MerchantId is required for Cashier role"));
                case "Cashier":
                {
                    var merchantId = Guid.Parse(request.MerchantId);
                    var merchantExists = await _db.Merchants.AnyAsync(m => m.Id == merchantId);
                    if (!merchantExists)
                        return Result.Failure(new NotFoundError(errCode, "Merchant does not exist"));

                    user.MerchantId = merchantId;
                    break;
                }
            }

            await _db.Users.AddAsync(user);
            var result = await _db.SaveChangesAsync();
            return result > 0
                ? Result.Success()
                : Result.Failure(new InternalError("User.Create", "Failed to create user"));
        }
        catch (Exception e)
        {
            return Result.Failure(new InternalError(errCode, e.Message));
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

public class UserGetByIdResponseDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
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
}

public class UserUpdateRequestDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
}