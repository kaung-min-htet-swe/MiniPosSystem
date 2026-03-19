namespace MiniPos.Frontend.Models;

public class CategoryCreateRequestDto
{
    public string MerchantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}