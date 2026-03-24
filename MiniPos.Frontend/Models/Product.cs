namespace MiniPos.Frontend.Models;

public class Product
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
