namespace MiniPos.Frontend.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public List<BranchInventory> BranchInventories { get; set; } = new();
}
