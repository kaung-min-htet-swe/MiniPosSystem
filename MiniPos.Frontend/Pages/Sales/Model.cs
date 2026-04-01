namespace MiniPos.Frontend.Pages.Sales;

class Order
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}

class OrderItem
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
}

class Merchant
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}