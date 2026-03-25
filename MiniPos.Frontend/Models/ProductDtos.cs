namespace MiniPos.Frontend.Models;

public class ProductListResponseDto
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

public class ProductGetByIdResponseDto : ProductListResponseDto
{
    public MerchantDto? Merchant { get; set; }
    public CategoryDto? Category { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class ProductCreateRequestDto
{
    public Guid MerchantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
}

public class ProductUpdateRequestDto
{
    public Guid? CategoryId { get; set; }
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
}

public class ProductCreateResponseDto
{
    public Guid Id { get; set; }
}
