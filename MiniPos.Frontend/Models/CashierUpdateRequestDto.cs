namespace MiniPos.Frontend.Models;

public class CashierUpdateRequestDto
{
    public string Email { get; set; } = string.Empty;
    public Guid? MerchantId { get; set; }
    public Guid? BranchId { get; set; }
}