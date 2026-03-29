using Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniPos.Backend.Extensions;

namespace MiniPos.Backend.Features.Auth;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthenticationController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        var processedById = User.GetUserId();
        request.ProcessedById = processedById;
        
        var result = await _authService.Signup(request);
        if (result is { IsSuccess: true, Data: {Token: not null} })
        {
            SetAuthCookies(result.Data.Token, result.Data.Id);
            return Created($"api/users/{result.Data!.Id}", result.Data);
        }

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });
    }

    [HttpPost("signin")]
    public async Task<IActionResult> Signin([FromBody] SigninRequest request)
    {
        var result = await _authService.Signin(request);
        if (result is { IsSuccess: true, Data:  {Token: not null} } )
        {
            SetAuthCookies(result.Data!.Token, result.Data.Id);
            return Ok(result.Data.Token);
        }

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });
    }

    [HttpPost("signout")]
    public async Task<IActionResult> Signout()
    {
        var result = await _authService.Signout();
        if (result.IsSuccess)
        {
            // The service handles deleting the 'token' cookie, 
            // but the controller was deleting these other cookies.
            // Keeping both for now or consolidating if appropriate.
            Response.Cookies.Delete("X-Access-Token");
            Response.Cookies.Delete("X-Refresh-Token");
            Response.Cookies.Delete("X-User-Id");
            return Ok(result.Data);
        }
        return Ok(new { message = "Logged out successfully" });
    }

    private void SetAuthCookies(TokenResponse tokenResponse, Guid userId)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = tokenResponse.AccessTokenExpiresAtUtc
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = tokenResponse.RefreshTokenExpiresAtUtc
        };

        Response.Cookies.Append("X-Access-Token", tokenResponse.AccessToken, accessCookieOptions);
        Response.Cookies.Append("X-Refresh-Token", tokenResponse.RefreshToken, refreshCookieOptions);
        Response.Cookies.Append("X-User-Id", userId.ToString(), new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = tokenResponse.AccessTokenExpiresAtUtc
        });
    }
}