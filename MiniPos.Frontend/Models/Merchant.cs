namespace MiniPos.Frontend.Models;

public class Merchant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public List<Branch> Branches { get; set; } = new();
}
