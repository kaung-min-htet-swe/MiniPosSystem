using Mapper;
using Microsoft.AspNetCore.Mvc;

namespace MiniPos.Backend.Features.Users;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] UserListRequestDto filter)
    {
        var result = await _userService.GetList(filter);
        if (!result.IsSuccess)
        {
            var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
            return StatusCode(statusCode, result.Error);
        }

        var pagedResult = result.Data!;
        if (pagedResult.TotalCount == 0)
        {
            return NotFound(pagedResult);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] string role)
    {
        var result = await _userService.GetById(new UserGetByIdRequestDto{Id = id, Role = role});
        if (result.IsSuccess)
            return Ok(result.Data);
        
        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateRequestDto request)
    {
        var result = await _userService.Create(request);
        if (result.IsSuccess)
        {
            Console.WriteLine("User created successfully.");
            return Created();
        }

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequestDto request)
    {
        var result = await _userService.Update(id, request);
        if (result.IsSuccess)
            return Ok();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.Delete(id);
        if (result.IsSuccess)
            return NoContent();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, new { message = result.Error!.Message });
    }
}