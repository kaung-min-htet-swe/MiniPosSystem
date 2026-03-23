using Mapper;
using Microsoft.AspNetCore.Mvc;

namespace MiniPos.Backend.Features.BranchInventories;

[ApiController]
[Route("api/products")]
public class BranchInventoryController : ControllerBase
{
    private readonly IBranchInventoryService _branchInventoryService;

    public BranchInventoryController(IBranchInventoryService branchInventoryService)
    {
        _branchInventoryService = branchInventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] BranchInventoryListRequestDto filter)
    {
        var result = await _branchInventoryService.GetList(filter);
        if (result.IsSuccess)
            return Ok(result.Data);

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _branchInventoryService.GetById(id);
        if (result.IsSuccess)
            return Ok(result.Data);

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] BranchInventoryCreateRequestDto request)
    {
        var result = await _branchInventoryService.Create(request);
        if (result.IsSuccess)
            return Created();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BranchInventoryUpdateRequestDto request)
    {
        var result = await _branchInventoryService.Update(id, request);
        if (result.IsSuccess)
            return Ok();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _branchInventoryService.Delete(id);
        if (result.IsSuccess)
            return NoContent();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }
}