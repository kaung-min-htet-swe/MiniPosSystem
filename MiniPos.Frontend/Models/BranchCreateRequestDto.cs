namespace MiniPos.Frontend.Models;

public class BranchCreateRequestDto
{
    public string MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
}