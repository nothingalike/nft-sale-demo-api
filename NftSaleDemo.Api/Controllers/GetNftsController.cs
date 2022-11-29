using Microsoft.AspNetCore.Mvc;
using NftSaleDemo.Api.Data;
using NftSaleDemo.Api.Enums;
using NftSaleDemo.Api.Services;

namespace NftSaleDemo.Api.Controllers;

[Route("api/get-nfts")]
public class GetNftsController : Controller
{
    private readonly INftRepository _nftService;

    public GetNftsController(INftRepository nftService)
    {
        _nftService = nftService;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        return Ok(await _nftService.GetNftsForSale());
    }
}