namespace MiniPos.Frontend.Models;

public class Branch
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}
