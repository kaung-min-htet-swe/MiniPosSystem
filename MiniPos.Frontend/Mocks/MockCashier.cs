using MiniPos.Frontend.Models;

namespace MiniPos.Frontend.Mocks;

public interface IMockCashier
{
    PagedResult<CashierResponseDto> GetList(string? merchantId);
    CashierResponseDto GetById(string id);
    bool Create(CashierCreateRequestDto cashier);
}

public class MockCashier : IMockCashier
{
    private readonly List<Cashier> _cashiers = new();
    private readonly IMockBranch _mockBranch;
    private readonly IMockMerchant _mockMerchant;

    public MockCashier(IMockMerchant merchant, IMockBranch branch)
    {
        _mockMerchant = merchant;
        var merchants = _mockMerchant.GetList().Data;
        var index = 0;
        foreach (var m in merchants)
        {
            _mockBranch = branch;
            var branches = _mockBranch.GetList(m.Id).Data;
            foreach (var b in branches)
            {
                _cashiers.Add(
                    new Cashier
                    {
                        Id = Guid.NewGuid(),
                        Username = $"cashier{index}",
                        Merchant = m,
                        Branch = new BranchResponseDto
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Address =b.Address
                        },
                        CreatedAt = DateTime.UtcNow,
                        Email = $"cashier{index}@gmail.com"
                    }); 
            }

            index++;
        }
    }

    public PagedResult<CashierResponseDto> GetList(string? merchantId)
    {
        var query = _cashiers.AsQueryable();
        if (merchantId != null)
        {
            query = query.Where(c => c.Merchant.Id.ToString() == merchantId);
        }
        var cashiers = query.Select(c => new CashierResponseDto
            {
                Id = c.Id.ToString(),
                Username = c.Username,
                Email = c.Email,
                Branch = new BranchResponseDto
                {
                    Id = c.Branch.Id,
                    Name = c.Branch.Name,
                    Address = c.Branch.Address
                },
                Merchant = new MerchantResponseDto
                {
                    Id = c.Merchant.Id.ToString(),
                    Name = c.Merchant.Name
                },
                CreatedAt = c.CreatedAt
            }).ToList();

        return new PagedResult<CashierResponseDto>(cashiers, cashiers.Count, 1, 10);
    }

    public CashierResponseDto GetById(string id)
    {
        return _cashiers.Where(c => c.Id.ToString() == id)
            .Select(c => new CashierResponseDto
            {
                Id = c.Id.ToString(),
                Username = c.Username,
                Email = c.Email,
                Branch = new BranchResponseDto
                {
                    Id = c.Branch.Id,
                    Name = c.Branch.Name,
                    Address = c.Branch.Address
                },
                Merchant = new MerchantResponseDto
                {
                    Id = c.Merchant.Id.ToString(),
                    Name = c.Merchant.Name
                },
                CreatedAt = c.CreatedAt
            }).FirstOrDefault() ?? new CashierResponseDto();
    }

    public bool Create(CashierCreateRequestDto cashier)
    {
        Console.WriteLine($"Creating cashier for merchant {cashier.MerchantId} with username {cashier.Username}");
        return true;
    }
}

public class Cashier
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public BranchResponseDto Branch { get; set; } = null!;
    public MerchantResponseDto Merchant { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}