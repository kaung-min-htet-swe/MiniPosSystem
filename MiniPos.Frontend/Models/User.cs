namespace MiniPos.Frontend.Models;

public class User
{
    public Guid Id { get; set; }
    public Merchant? Merchant { get; set; }
    public Branch? Branch { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}
