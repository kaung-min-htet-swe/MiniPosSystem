namespace MiniPos.Frontend.Models;

public class ProductRequestDto
{
    public string Id { get; set; } = string.Empty;
    public MerchantResponseDto Merchant { get; set; } = null;
    public CategoryResponseDto Category { get; set; } = null;
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; } 
    public DateTime CreatedAt { get; set; }
}