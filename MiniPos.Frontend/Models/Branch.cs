using System.Collections.Generic;

namespace MiniPos.Frontend.Models;

public class Branch
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual Merchant? Merchant { get; set; }
}
