using Common;
using Mapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MiniPos.Backend.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthenticationController:ControllerBase
{
    private readonly IAuthenticationService _authService;
    
    public AuthenticationController(IAuthenticationService authService)
    {
        _authService = authService;
    }
    
    [HttpPost]
    [Route("/signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequestDto request)
    {
        var result = await _authService.Signup(request);
        if (result.IsSuccess) return Created($"api/users/{result.Data!.Id}", result.Data);
        
        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });

    }

    [HttpPost]
    [Route("/signin")]
    public async Task<IActionResult> Signin([FromBody] SigninRequestDto request)
    {
        var result = await _authService.Signin(request);
        if (result.IsSuccess) return Ok(result.Data);
        
        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });
    }

    [HttpPost]
    [Route("/signout")]
    public async Task<IActionResult> Signout([FromBody] SignoutRequestDto request)
    {
        throw new NotImplementedException();
    }
}