namespace MiniPos.Frontend.Models;

public class User
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? TotalSales { get; set; }
    public Merchant? Merchant { get; set; }
    public Branch? Branch { get; set; }
    public List<Branch> Branches { get; set; } = new();
    public DateTime? JoinedDate { get; set; }
}