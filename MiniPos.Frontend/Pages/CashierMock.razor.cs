using MiniPos.Frontend.Models;
using MudBlazor;

namespace MiniPos.Frontend.Pages;

public partial class Cashier
{
    private List<CashierResponseDto> _mockcashiers = [];

    private List<CashierResponseDto> GenerateCashiers()
    {
        var cashiers = new List<CashierResponseDto>();

        for (var i = 0; i < 10; i++)
            cashiers.Add(new CashierResponseDto
            {
                Id = Guid.NewGuid().ToString(),
                Username = $"Cashier {i}",
                Email = $"Cashier.{i}@gmail.com",
                CreatedAt = DateTime.Now,
                Merchant = new MerchantResponseDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Merchant A"
                },
                Branch = new BranchResponseDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Branch A"
                }
            });

        return cashiers;
    }

    private async Task<TableData<CashierResponseDto>> MockCashierReload(TableState state, CancellationToken token)
    {
        await Task.Delay(1000);
        _mockcashiers = GenerateCashiers();

        var response = new
        {
            TotalCount = _mockcashiers.Count,
            Data = _mockcashiers
        };

        return new TableData<CashierResponseDto>
        {
            TotalItems = response.TotalCount,
            Items = response.Data
        };
    }
}