namespace MiniPos.Frontend.Models;

public class CashierResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public BranchResponseDto? Branch { get; set; } = null;
    public MerchantResponseDto? Merchant { get; set; } = null;
    public DateTime CreatedAt { get; set; }
}