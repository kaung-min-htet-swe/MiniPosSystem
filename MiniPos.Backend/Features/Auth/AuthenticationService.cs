using Common;
using Database.EfAppDbContextModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniPos.Backend.Features.Users;

namespace MiniPos.Backend.Features.Auth;

public interface IAuthenticationService
{
    Task<Result<SignupResponse>> Signup(SignupRequest request);
    Task<Result<SigninResponse>> Signin(SigninRequest request);
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

    public async Task<Result<SignupResponse>> Signup(SignupRequest request)
    {
        var user = await _userService.Create(new UserCreateRequest
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

        if (!user.IsSuccess) return Result<SignupResponse>.Failure(user.Error!);

        var token = await _tokenService.IssueTokenAsync(request.UserName);
        var result = new SignupResponse
        {
            Id = user.Data!.Id,
            Token = token
        };

        return Result<SignupResponse>.Success(result);
    }

    public async Task<Result<SigninResponse>> Signin(SigninRequest request)
    {
        var errCode = "AuthenticationService.Signin";
        try
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == request.Email);
            if(user is null) return Result<SigninResponse>.Failure(new UnAuthorized(errCode, "Invalid credentials"));
            
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if(result == PasswordVerificationResult.Failed) 
                return  Result<SigninResponse>.Failure(new UnAuthorized(errCode, "Invalid credentials"));
            
            var token = await _tokenService.IssueTokenAsync(user.Username);
            return Result<SigninResponse>.Success(new SigninResponse
            {
                Id = user.Id,
                Token = token
            });

        }
        catch (Exception e)
        {
            return Result<SigninResponse>.Failure(new InternalError(errCode, e.Message));
        }
    }

    public Task<Result<SignoutResponseDto>> Signout(SignoutRequestDto request)
    {
        throw new NotImplementedException();
    }
}

public class SignupRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? Password { get; set; }
    public Guid MerchantId { get; set; }
    public Guid ProcessedById { get; set; }
    public Guid BranchId { get; set; }
    public MerchantCreationDto? Merchant { get; set; }
}

public class SignupResponse
{
    public Guid Id { get; set; }
    public TokenResponse? Token { get; set; }
}

public class SigninRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class SigninResponse
{
    public Guid Id { get; set; }
    public TokenResponse? Token { get; set; }
}

public class SignoutRequestDto
{
}

public class SignoutResponseDto
{
}