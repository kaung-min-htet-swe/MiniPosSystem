using Common;
using Database.EfAppDbContextModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniPos.Backend.Features.Users;

namespace MiniPos.Backend.Features.Auth;

public interface IAuthenticationService
{
    Task<Result<SignupResponseDto>> Signup(SignupRequestDto request);
    Task<Result<SigninResponseDto>> Signin(SigninRequestDto request);
    Task<Result<SignoutResponseDto>> Signout(SignoutRequestDto request);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly TokenService _tokenService;
    private readonly IUserService _userService;

    public AuthenticationService(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IUserService userService,
        TokenService tokenService
    )
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<Result<SignupResponseDto>> Signup(SignupRequestDto request)
    {
        var user = await _userService.Create(new UserCreateRequestDto
        {
            UserName = request.UserName,
            Email = request.Email,
            Role = request.Role,
            Password = request.Password,
            Merchant = request.Merchant,
            MerchantId = request.MerchantId,
            ProcessedById = request.ProcessedById,
            BranchId = request.BranchId
        });

        if (!user.IsSuccess) return Result<SignupResponseDto>.Failure(user.Error!);

        var token = await _tokenService.IssueTokenAsync(request.UserName);
        var result = new SignupResponseDto
        {
            Id = user.Data!.Id,
            Token = token
        };

        return Result<SignupResponseDto>.Success(result);
    }

    public async Task<Result<SigninResponseDto>> Signin(SigninRequestDto request)
    {
        var errCode = "AuthenticationService.Signin";
        try
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == request.Email);
            if(user is null) return Result<SigninResponseDto>.Failure(new UnAuthorized(errCode, "Invalid credentials"));
            
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if(result == PasswordVerificationResult.Failed) 
                return  Result<SigninResponseDto>.Failure(new UnAuthorized(errCode, "Invalid credentials"));
            
            var token = await _tokenService.IssueTokenAsync(user.Username);
            return Result<SigninResponseDto>.Success(new SigninResponseDto
            {
                Id = user.Id,
                Token = token
            });

        }
        catch (Exception e)
        {
            return Result<SigninResponseDto>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public Task<Result<SignoutResponseDto>> Signout(SignoutRequestDto request)
    {
        throw new NotImplementedException();
    }
}

public class SignupRequestDto
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public required string Password { get; set; }
    public MerchantCreateDto? Merchant { get; set; }
    public string? MerchantId { get; set; }
    public string? ProcessedById { get; set; }
    public string? BranchId { get; set; }
}

public class SignupResponseDto
{
    public required Guid Id { get; set; }
    public required TokenResponse Token { get; set; }
}

public class SigninRequestDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class SigninResponseDto
{
    public required Guid Id { get; set; }
    public required TokenResponse Token { get; set; }
}

public class SignoutRequestDto
{
}

public class SignoutResponseDto
{
}