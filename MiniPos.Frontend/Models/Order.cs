namespace MiniPos.Frontend.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ProcessedById { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid MerchantId { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
