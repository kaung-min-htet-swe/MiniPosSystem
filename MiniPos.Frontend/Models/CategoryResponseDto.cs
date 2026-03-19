namespace MiniPos.Frontend.Models;

public class CategoryResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MerchantResponseDto? Merchant { get; set; }
}