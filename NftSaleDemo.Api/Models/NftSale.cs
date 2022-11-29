using System.ComponentModel.DataAnnotations.Schema;

namespace NftSaleDemo.Api.Models;

[Table("NftSales")]
public class NftSale
{
    public int Id { get; set; }
    public string? TransactionCbor { get; set; }
    public string? NftSaleStatus { get; set; }
    public string? TransactionResult { get; set; }
}