namespace MiniPos.Frontend.Models;

public class CashierCreateRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
}