using System.ComponentModel.DataAnnotations.Schema;

namespace NftSaleDemo.Api.Models;

[Table("Nfts")]
public class Nft
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public string Name { get; set; }
    public string Rarity { get; set; }
    public long BaseCost { get; set; }
    public int Quantity { get; set; }
    public int StepCost { get; set; }
    public int StepQuantity { get; set; }
}