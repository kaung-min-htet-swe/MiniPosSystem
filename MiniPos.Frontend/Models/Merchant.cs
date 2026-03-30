namespace MiniPos.Frontend.Models;

public class Merchant
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int BranchCount { get; set; }
}
