namespace MiniPos.Frontend.Models;

public class CashierGetByIdResponseDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual BranchResponseDto? Branch { get; set; }
    public virtual MerchantResponseDto? Merchant { get; set; }
}