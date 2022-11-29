using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Extensions.Models.Transactions;
using CardanoSharp.Wallet.Utilities;
using Microsoft.AspNetCore.Mvc;
using NftSaleDemo.Api.Data;
using NftSaleDemo.Api.Enums;
using NftSaleDemo.Api.Models;
using NftSaleDemo.Api.Services;

namespace NftSaleDemo.Api.Controllers;

[Route("/api/transaction")]
public class TransactionController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly INftSaleRepository _nftSaleRepository;
    private readonly INftRepository _nftRepository;
    
    // GET
    public TransactionController(ITransactionService transactionService, INftRepository nftRepository, INftSaleRepository nftSaleRepository)
    {
        _transactionService = transactionService;
        _nftRepository = nftRepository;
        _nftSaleRepository = nftSaleRepository;
    }
    
    [HttpPost("build")]
    public async Task<IActionResult> BuildTransactionAsync([FromBody] BuildTransactionRequest request)
    {
        try
        {
            var rarityType = Enum.Parse<RarityType>(request.Rarity);
            var nft = await _nftRepository.GetByRarity(rarityType);
            if (nft is null)
                return NotFound("Nft not found");

            var transaction = await _transactionService.BuildTransactionForNftSale(request.AddressByte, nft);

            var nftSale = await _nftSaleRepository
                .SaveAsync(new NftSale()
                {
                    TransactionCbor = transaction.Serialize().ToStringHex(),
                    NftSaleStatus = NftSaleStatus.Pending.ToString()
                });

            // transaction.AuxiliaryData = null;
            // nftSale.TransactionCbor = transaction.Serialize().ToStringHex();
            return Ok(new
            {
                nftSale = nftSale, 
                bodyHash = HashUtility
                    .Blake2b256(transaction.TransactionBody.GetCBOR(transaction.AuxiliaryData)
                    .EncodeToBytes())
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("sign")]
    public async Task<IActionResult> SignTransactionAsync([FromBody] SignTransactionRequest request)
    {
        try
        {
            var nftSale = await _nftSaleRepository.FindAsync(request.NftSaleId);
            var transaction = await _transactionService.SignTransactionForNftSale(nftSale, request.Witness);

            nftSale.TransactionCbor = transaction.Serialize().ToStringHex();
            nftSale = await _nftSaleRepository.SaveAsync(nftSale);
            
            return Ok(new { nftSale = nftSale });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
}

public class BuildTransactionRequest
{
    public string AddressByte { get; set; }
    public string Rarity { get; set; }
}

public class SignTransactionRequest
{
    public string Witness { get; set; }
    public int NftSaleId { get; set; }
}