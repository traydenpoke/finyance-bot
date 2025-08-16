using FinyanceApp.Data;
using FinyanceApp.Models;
using FinyanceApp.Models.Enums;
using FinyanceApp.Results;
using FinyanceApp.Services;

namespace FinyanceApp.Database
{
  public class AssetService
  {
    private readonly PostgresService _postgres;
    private readonly GoogleFinanceService _financeService;
    private readonly AccountService _accountService;

    public AssetService(PostgresService postgres, GoogleFinanceService financeService, AccountService accountService)
    {
      _postgres = postgres;
      _financeService = financeService;
      _accountService = accountService;
    }

    public async Task<ServiceResult<decimal>> GetPriceAsync(string symbol, AssetType type)
    {
      var price = await _financeService.GetPriceAsync(symbol, type.ToString().ToLower());

      if (price.HasValue)
        return ServiceResult<decimal>.Ok(price.Value, $"${price.Value}");

      return ServiceResult<decimal>.Fail("Could not fetch price.");
    }

    public async Task<ServiceResult<Asset>> AddStockAsync(long userId, int accountId, string symbol, decimal amount, decimal cost)
    {
      // ensure account exists
      var account = await _accountService.GetAccountAsync(userId, accountId);
      if (!account.Success) return ServiceResult<Asset>.Fail("Account for this user by this id not found!");

      // ensure this asset exists - we will know it exists by trying to fetch description
      var type = account.Data.Type;
      var description = await _financeService.GetDescriptionAsync(symbol, type);
      if (description == null) return ServiceResult<Asset>.Fail("Symbol does not exist on Google Finance!");

      // add addet for this account
      var newAsset = new Asset
      {
        AccountId = accountId,
        UserId = userId,
        Symbol = symbol,
        Description = description,
        Type = type,
        Amount = amount,
        Cost = cost
      };

      await _postgres.AddAsync(TableName.Assets, newAsset);
      return ServiceResult<Asset>.Ok(newAsset, $"Added asset to account.");
    }

    // TODO: improve with specific SQL query GROUP BY
    public async Task<ServiceResult<decimal>> GetPortfolioAsync(long userId)
    {
      // get all assets
      var fields = new Dictionary<string, object> { { "user_id", userId } };
      var assets = await _postgres.GetByFieldsAsync<Asset>(TableName.Assets, fields);
      if (assets.Count == 0) return ServiceResult<decimal>.Fail("Failed to fetch assets for user.");

      // for each asset, get price
      decimal value = 0;
      foreach (var asset in assets)
      {
        var price = await _financeService.GetPriceAsync(asset.Symbol, asset.Type);
        Console.WriteLine(price);

        if (price == null) continue;

        value += (decimal)price * asset.Amount;
      }

      return ServiceResult<decimal>.Ok(value, $"Portfolio value: ${value}.");
    }

  }
}
