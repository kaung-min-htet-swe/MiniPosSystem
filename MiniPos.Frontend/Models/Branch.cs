using System.Collections.Generic;

namespace MiniPos.Frontend.Models;

public class Branch
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual Merchant? Merchant { get; set; }
    public virtual ICollection<BranchInventory> BranchInventories { get; set; } = new List<BranchInventory>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
