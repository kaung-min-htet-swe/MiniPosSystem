namespace MiniPos.Frontend.Models;

public class BranchListResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public MerchantDto? Merchant { get; set; }
}

public class BranchGetByIdResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public DateTime? CreatedAt { get; set; }
    public MerchantDto? Merchant { get; set; }
    public decimal TodayOrderPrice { get; set; }
    public int TodayOrderCount { get; set; }
}

public class BranchCreateRequestDto
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}

public class BranchUpdateRequestDto
{
    public string MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}

public class UserAssignBranchRequestDto
{
    public Guid BranchId { get; set; }
}
