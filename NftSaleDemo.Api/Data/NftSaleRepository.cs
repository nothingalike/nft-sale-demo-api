using NftSaleDemo.Api.Models;

namespace NftSaleDemo.Api.Data;

public interface INftSaleRepository
{
    Task<NftSale> SaveAsync(NftSale nftSale);
    Task<NftSale?> FindAsync(int id);
}

public class NftSaleRepository: INftSaleRepository
{
    private readonly NftSaleDemoContext _dbContext;

    public NftSaleRepository(NftSaleDemoContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NftSale> SaveAsync(NftSale nftSale)
    {
        NftSale? model;
        if (nftSale.Id > 0)
        {
            model = await _dbContext.NftSales.FindAsync(nftSale.Id);
            if (model is null) throw new Exception("Unable to find nft sale");
            model.TransactionCbor = nftSale.TransactionCbor;
            model.NftSaleStatus = nftSale.NftSaleStatus;
            model.TransactionResult = nftSale.TransactionResult;
            _dbContext.NftSales.Update(model);
        }
        else
        {
            model = new NftSale()
            {
                TransactionCbor = nftSale.TransactionCbor,
                NftSaleStatus = nftSale.NftSaleStatus,
                TransactionResult = nftSale.TransactionResult
            };
            await _dbContext.NftSales.AddAsync(model);
        }

        await _dbContext.SaveChangesAsync();

        return model;
    }

    public async Task<NftSale?> FindAsync(int id) =>
        await _dbContext.NftSales.FindAsync(id);
}