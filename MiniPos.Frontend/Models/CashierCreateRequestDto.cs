namespace MiniPos.Frontend.Models;

public class CashierCreateRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid? MerchantId { get; set; }
}