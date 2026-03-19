using MiniPos.Frontend.Models;

namespace MiniPos.Frontend.Pages;

public partial class CashierView
{
    private Task GetCashierMock(string id)
    {
        _cashier = new CashierGetByIdResponseDto
        {
            Id = id,
            Username = "john_doe",
            Email = "johndoe@gmail.com",
            Merchant = new MerchantResponseDto
            {
                Id = "merchant-123",
                Name = "John's Store"
            },
            Branch = new BranchResponseDto
            {
                Id = "branch-456",
                Name = "Main Branch"
            },
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            Role = "Cashier"
        };
        _isLoading = false;
        return Task.CompletedTask;
    }
}