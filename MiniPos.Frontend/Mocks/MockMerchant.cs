using MiniPos.Frontend.Models;

namespace MiniPos.Frontend.Mocks;

public interface IMockMerchant
{
    PagedResult<MerchantResponseDto> GetList();
    MerchantResponseDto? GetById(string id);
}

public class MockMerchant : IMockMerchant
{
    private readonly List<Merchant> _merchants = new();

    public MockMerchant()
    {
        _merchants.Add(new Merchant { Id = Guid.NewGuid(), Name = "Merchant 1", ContactEmail = "merchant1@gmail.com" });
        _merchants.Add(new Merchant { Id = Guid.NewGuid(), Name = "Merchant 2", ContactEmail = "merchant2@gmail.com" });
        _merchants.Add(new Merchant { Id = Guid.NewGuid(), Name = "Merchant 3", ContactEmail = "merchant3@gmail.com" });
        _merchants.Add(new Merchant { Id = Guid.NewGuid(), Name = "Merchant 4", ContactEmail = "merchant4@gmail.com" });
    }

    public PagedResult<MerchantResponseDto> GetList()
    {
        var merchants = _merchants.Select(m => new MerchantResponseDto { Id = m.Id.ToString(), Name = m.Name })
            .ToList();
        return new PagedResult<MerchantResponseDto>(merchants, merchants.Count, 1, 10);
    }

    public MerchantResponseDto? GetById(string id)
    {
        return _merchants
            .Where(m => m.Id.ToString() == id)
            .Select(m => new MerchantResponseDto { Id = m.Id.ToString(), Name = m.Name })
            .FirstOrDefault();
    }
}

public class Merchant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}