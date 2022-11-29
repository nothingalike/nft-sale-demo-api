using Microsoft.EntityFrameworkCore;
using NftSaleDemo.Api.Models;

namespace NftSaleDemo.Api.Data;

public class NftSaleDemoContext: DbContext
{
    public NftSaleDemoContext()
    {
        
    }

    public NftSaleDemoContext(DbContextOptions<NftSaleDemoContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseNpgsql()
            .UseSnakeCaseNamingConvention();

    public DbSet<Nft> Nfts { get; set; }
    public DbSet<NftSale> NftSales { get; set; }
}