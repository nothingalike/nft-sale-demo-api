using Microsoft.EntityFrameworkCore;
using NftSaleDemo.Api.Enums;
using NftSaleDemo.Api.Models;

namespace NftSaleDemo.Api.Data;

public interface INftRepository
{
    Task<List<Nft>> GetNftsForSale();
    Task AdjustNft(Nft nft, int amount);
    Task<Nft?> GetByRarity(RarityType rarityType);
}

public class NftRepository: INftRepository
{
    private readonly NftSaleDemoContext _dbContext;

    public NftRepository(NftSaleDemoContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Nft>> GetNftsForSale() =>
        await _dbContext.Nfts.ToListAsync();

    public Task AdjustNft(Nft nft, int amount)
    {
        throw new NotImplementedException();
    }

    public async Task<Nft?> GetByRarity(RarityType rarityType) =>
        await _dbContext.Nfts.FirstOrDefaultAsync(x => x.Rarity == rarityType.ToString());
}