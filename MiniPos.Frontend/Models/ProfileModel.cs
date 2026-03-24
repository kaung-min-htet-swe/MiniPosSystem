using System;

namespace MiniPos.Frontend.Models;

public class ProfileModel
{
    public Guid Id { get; set; }

    public Guid? MerchantId { get; set; }

    public Guid? BranchId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Merchant? Merchant { get; set; }

    public virtual Branch? Branch { get; set; }
}
