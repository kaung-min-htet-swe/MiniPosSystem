using MiniPos.Frontend.Models;
using MudBlazor;

namespace MiniPos.Frontend.Pages;

public partial class Category
{
    public async Task<TableData<CategoryResponseDto>> MockServerReload(TableState state, CancellationToken token)
    {
        var pageNumber = state.Page + 1;
        var pageSize = state.PageSize;

        List<CategoryResponseDto> categories = [];
        foreach (var i in Enumerable.Range(1, 10))
        {
            categories.Add(
                new CategoryResponseDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = $"Desciption {i}",
                    Merchant = new MerchantResponseDto { Id = Guid.NewGuid().ToString(), Name = $"Merchant {i}" },
                    Name = $"Category {i}"
                }
            );
        }

        var response = new { TotalCount = categories.Capacity, Data = categories.ToArray() };
        return new TableData<CategoryResponseDto>
        {
            TotalItems = response.TotalCount,
            Items = response.Data
        };
    }
}