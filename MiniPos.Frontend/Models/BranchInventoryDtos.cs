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

public class BranchInventoryCreateRequestDto
{
    public Guid MerchantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public int StockQuantity { get; set; }
}

public class BranchInventoryUpdateRequestDto
{
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
}
