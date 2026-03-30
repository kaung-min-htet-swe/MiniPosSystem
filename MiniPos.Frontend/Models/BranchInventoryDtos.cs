namespace MiniPos.Frontend.Models;

public class BranchInventoryListResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? CategoryName { get; set; }
}
