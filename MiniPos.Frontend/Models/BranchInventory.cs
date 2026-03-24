namespace MiniPos.Frontend.Models;

public class BranchInventory
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public int StockQuantity { get; set; }
}
