namespace MiniPos.Frontend.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Customer? Customer { get; set; }
    public User? ProcessedBy { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
