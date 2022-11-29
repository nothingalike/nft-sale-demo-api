using System.Text;
using CardanoSharp.Koios.Client;
using CardanoSharp.Wallet.CIPs.CIP2;
using CardanoSharp.Wallet.CIPs.CIP2.Models;
using CardanoSharp.Wallet.Encoding;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Extensions.Models.Transactions;
using CardanoSharp.Wallet.Extensions.Models.Transactions.TransactionWitnesses;
using CardanoSharp.Wallet.Models.Transactions;
using CardanoSharp.Wallet.Models;
using CardanoSharp.Wallet.Models.Addresses;
using CardanoSharp.Wallet.TransactionBuilding;
using NftSaleDemo.Api.Models;
using CardanoSharpAsset = CardanoSharp.Wallet.Models.Asset;

namespace NftSaleDemo.Api.Services;

public interface ITransactionService
{
    Task<Transaction?> BuildTransactionForNftSale(string addressBytes, Nft nft);
    Task<Transaction?> SignTransactionForNftSale(NftSale nftSale, string witness);
}

public class TransactionService: ITransactionService
{
    private readonly IAddressClient _addressClient;
    private readonly INetworkClient _networkClient;
    private readonly IEpochClient _epochClient;
    private readonly IPolicyManager _policyManager;
    private readonly string _sendPaymentToAddress;

    public TransactionService(IConfiguration config, IAddressClient addressClient, INetworkClient networkClient, IEpochClient epochClient, IPolicyManager policyManager)
    {
        _addressClient = addressClient;
        _networkClient = networkClient;
        _epochClient = epochClient;
        _policyManager = policyManager;
        _sendPaymentToAddress = config["Cardano:SendPaymentToAddress"];
    }

    public async Task<Transaction?> BuildTransactionForNftSale(string addressBytes, Nft nft)
    {
        //0. Prep
        var address = new Address(addressBytes.HexToByteArray());
        var scriptPolicy = _policyManager.GetPolicyScript();
        
        //1. Get UTxOs
        var utxos = await GetUtxos(address.ToString());

        ///2. Create the Body
        var transactionBody = TransactionBodyBuilder.Create;
        
        //set payment outputs
        transactionBody.AddOutput(_sendPaymentToAddress.ToAddress().GetBytes(), (ulong)(nft.BaseCost));
        
        //set mint
        var policyId = scriptPolicy.Build().GetPolicyId();
        ITokenBundleBuilder tbb = TokenBundleBuilder.Create
            .AddToken(policyId, nft.Name.ToBytes(), 1);
        transactionBody.AddOutput(address.GetBytes(), 2000000, tbb,  outputPurpose: OutputPurpose.Mint);
        transactionBody.SetMint(tbb);

        //perform coin selection
        var coinSelection = ((TransactionBodyBuilder)transactionBody).UseRandomImprove(utxos, address.ToString(), tbb);

        //add the inputs from coin selection to transaction body builder
        AddInputsFromCoinSelection(coinSelection, transactionBody);

        //if we have change from coin selection, add to outputs
        if (coinSelection.ChangeOutputs is not null && coinSelection.ChangeOutputs.Any())
        {
            AddChangeOutputs(transactionBody, coinSelection.ChangeOutputs, address.ToString());
        }

        //get protocol parameters and set default fee
        var ppResponse = await _epochClient.GetProtocolParameters();
        var protocolParameters = ppResponse.Content.FirstOrDefault();
        transactionBody.SetFee(protocolParameters.MinFeeB.Value);

        //get network tip and set ttl
        var blockSummaries = (await _networkClient.GetChainTip()).Content;
        var ttl = 2500 + (uint)blockSummaries.First().AbsSlot;
        transactionBody.SetTtl(ttl);

        ///3. Mock Witnesses
        var witnessSet = TransactionWitnessSetBuilder.Create
            .SetScriptAllNativeScript(scriptPolicy)
            .MockVKeyWitness(2);
        
        //metadata
        var metadata = GetMetadata(
            nft.Rarity, nft.Id, nft.Name, $"ipfs://{nft.ImageUrl}", policyId.ToStringHex());
        var auxData = AuxiliaryDataBuilder.Create
            .AddMetadata(721, metadata);

        ///4. Build Draft TX
        //create transaction builder and add the pieces
        var transaction = TransactionBuilder.Create;
        transaction.SetBody(transactionBody);
        transaction.SetWitnesses(witnessSet);
        transaction.SetAuxData(auxData);

        //get a draft transaction to calculate fee
        var draft = transaction.Build();
        var fee = draft.CalculateFee(protocolParameters.MinFeeA, protocolParameters.MinFeeB);

        //update fee and change output
        transactionBody.SetFee(fee);
        transactionBody.RemoveFeeFromChange();
        
        var rawTx = transaction.Build();
        
        //remove mock witness
        var mockWitnesses = rawTx.TransactionWitnessSet.VKeyWitnesses.Where(x => x.IsMock);
        foreach (var mw in mockWitnesses)
            rawTx.TransactionWitnessSet.VKeyWitnesses.Remove(mw);

        return rawTx;
    }

    public async Task<Transaction?> SignTransactionForNftSale(NftSale nftSale, string witness)
    {
        var transaction = nftSale.TransactionCbor.HexToByteArray().DeserializeTransaction();
        var vKeyWitnesses = witness.HexToByteArray().DeserializeTransactionWitnessSet();
        
        foreach(var vkeyWitness in vKeyWitnesses.VKeyWitnesses) 
            transaction.TransactionWitnessSet.VKeyWitnesses.Add(vkeyWitness);
        
        transaction.TransactionWitnessSet.VKeyWitnesses.Add(new VKeyWitness()
        {
            VKey = _policyManager.GetPublicKey(),
            SKey = _policyManager.GetPrivateKey()
        });

        return transaction;
    }

    //Helper Functions
    private async Task<List<Utxo>> GetUtxos(string address)
    {
        try
        {
            var addressBulkRequest = new AddressBulkRequest { Addresses = new List<string> { address } };
            var addressResponse = (await _addressClient.GetAddressInformation(addressBulkRequest));
            var addressInfo = addressResponse.Content;
            var utxos = new List<Utxo>();

            foreach (var ai in addressInfo.SelectMany(x => x.UtxoSets))
            {
                if(ai is null) continue;
                var utxo = new Utxo()
                {
                    TxIndex = ai.TxIndex,
                    TxHash = ai.TxHash,
                    Balance = new Balance()
                    {
                        Lovelaces = ulong.Parse(ai.Value)
                    }
                };

                var assetList = new List<CardanoSharpAsset>();
                foreach (var aa in ai.AssetList)
                {
                    assetList.Add(new CardanoSharpAsset()
                    {
                        Name = aa.AssetName,
                        PolicyId = aa.PolicyId,
                        Quantity = long.Parse(aa.Quantity)
                    });
                }

                utxo.Balance.Assets = assetList;
                utxos.Add(utxo);
            }

            return utxos;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private void AddInputsFromCoinSelection(CoinSelection coinSelection, ITransactionBodyBuilder transactionBody)
    {
        foreach (var i in coinSelection.Inputs)
        {
            transactionBody.AddInput(i.TransactionId, i.TransactionIndex);
        }
    }

    private void AddChangeOutputs(ITransactionBodyBuilder ttb, List<TransactionOutput> outputs, string address)
    {
        foreach (var output in outputs)
        {
            ITokenBundleBuilder? assetList = null;

            if (output.Value.MultiAsset is not null)
            {
                assetList = TokenBundleBuilder.Create;
                foreach (var ma in output.Value.MultiAsset)
                {
                    foreach (var na in ma.Value.Token)
                    {
                        assetList.AddToken(ma.Key, na.Key, na.Value);
                    }
                }
            }

            ttb.AddOutput(new Address(address), output.Value.Coin, assetList, outputPurpose: OutputPurpose.Change);
        }
    }

    private Dictionary<string, object> GetMetadata(string rarity, int id, string name, string image, string policyId)
    {
        var file = new
        {
            name = $"{name} Icon",
            mediaType = "image/png",
            src = image
        };
        var fileElement = new List<object>() { file };

        var assetElement = new Dictionary<string, object>()
        {
            {
                Encoding.ASCII.GetBytes($"{name} {rarity}").ToStringHex(), 
                new 
                {
                    name = name,
                    image = image,
                    mediaType = "image/png",
                    files = fileElement,
                    serialNum = $"SOD{rarity}{id}",
                    rarity = rarity
                }
            }
        };

        var policyElement = new Dictionary<string, object>()
        {
            {
                policyId, assetElement
            }
        };

        // return new Dictionary<string, object>()
        // {
        //     {
        //         "721", policyElement
        //     }
        // };
        return policyElement;
    }
}