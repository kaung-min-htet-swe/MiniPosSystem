namespace MiniPos.Frontend.Models;

public class Product
{
    public Guid Id { get; set; }
    public Merchant? Merchant { get; set; }
    public Category? Category { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
