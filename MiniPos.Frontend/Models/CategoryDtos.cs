namespace MiniPos.Frontend.Models;

public class CategoryListResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid MerchantId { get; set; }
    public string? MerchantName { get; set; }
    public int ProductCount { get; set; }
}

public class CategoryGetByIdResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public MerchantDto? Merchant { get; set; }
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    public DateTime CreatedAt { get; set; }
}

public class MerchantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactEmail { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
