using Mapper;
using Microsoft.AspNetCore.Mvc;

namespace MiniPos.Backend.Features.Products;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ProductListRequestDto filter)
    {
        var result = await _productService.GetList(filter);
        if (result.IsSuccess)
            return Ok(result.Data);

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetById(id);
        if (result.IsSuccess)
            return Ok(result.Data);

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequestDto request)
    {
        var result = await _productService.Create(request);
        if (result.IsSuccess)
            return Created();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateRequestDto request)
    {
        var result = await _productService.Update(id, request);
        if (result.IsSuccess)
            return Ok();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.Delete(id);
        if (result.IsSuccess)
            return NoContent();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }
}
