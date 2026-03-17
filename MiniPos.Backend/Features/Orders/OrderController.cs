using Mapper;
using Microsoft.AspNetCore.Mvc;

namespace MiniPos.Backend.Features.Orders;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] OrderListRequestDto filter)
    {
        var result = await _orderService.GetList(filter);
        if (result.IsSuccess)
            return Ok(result.Data);

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetById(id);
        if (result.IsSuccess)
            return Ok(result.Data);

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto request)
    {
        var result = await _orderService.Create(request);
        if (result.IsSuccess)
            return Created();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _orderService.Delete(id);
        if (result.IsSuccess)
            return NoContent();

        var statusCode = ErrorHttpMapper.GetStatusCode(result.Error!);
        return StatusCode(statusCode, result.Error);
    }
}
