using MiniPos.Frontend.Models;

namespace MiniPos.Frontend.Mocks;

public interface IMockBranch
{
    PagedResult<BranchResponseDto> GetList(string merchantId);
    bool Create(BranchCreateRequestDto model);
}

public class MockBranch : IMockBranch
{
    private readonly List<Branch> _branches = [];
    private readonly IMockMerchant _merchant;

    public MockBranch(IMockMerchant merchant)
    {
        _merchant = merchant;
        var merchants = _merchant.GetList().Data;
        foreach (var m in merchants)
        {
            _branches.Add(new Branch { Id = Guid.NewGuid(), Name = "Branch 1", Address = "Address 1", Merchant = m });
            _branches.Add(new Branch { Id = Guid.NewGuid(), Name = "Branch 2", Address = "Address 2", Merchant = m });
            _branches.Add(new Branch { Id = Guid.NewGuid(), Name = "Branch 3", Address = "Address 3", Merchant = m });
        }
    }

    public PagedResult<BranchResponseDto> GetList(string merchantId)
    {
        var m = _merchant.GetById(merchantId);
        if (m is null) return new PagedResult<BranchResponseDto>([], 0, 1, 10);

        var branches = _branches
            .Select(b =>
                new BranchResponseDto
                {
                    Id = b.Id.ToString(),
                    Name = b.Name,
                    Address = b.Address,
                    MerchantName = b.Merchant.Name
                }).ToList();

        return new PagedResult<BranchResponseDto>(branches, branches.Count, 1, 10);
    }

    public bool Create(BranchCreateRequestDto model)
    {
        Console.WriteLine(
            $"Creating branch for merchant {model.MerchantId} with name {model.Name} and address {model.Address}");
        return true;
    }
}

public class Branch
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public MerchantResponseDto Merchant { get; set; } = null!;
}