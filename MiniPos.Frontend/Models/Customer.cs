namespace MiniPos.Frontend.Models;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public Merchant? Merchant { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Order> Orders { get; set; } = new();
}
