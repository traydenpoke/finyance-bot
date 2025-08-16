using Discord.Interactions;
using FinyanceApp.Commands.Autocomplete;
using FinyanceApp.Database;
using FinyanceApp.Models.Enums;

namespace FinyanceApp.Commands
{
  [Group("asset", "Manage assets.")]
  public class AssetCommands : InteractionModuleBase<SocketInteractionContext>
  {
    private readonly AssetService _assetService;
    private readonly AccountService _accountService;

    public AssetCommands(AssetService assetService, AccountService accountService)
    {
      _assetService = assetService;
      _accountService = accountService;
    }

    [SlashCommand("price", "Get current price of an asset.")]
    public async Task PriceAsync(string symbol, AssetType type)
    {
      var result = await _assetService.GetPriceAsync(symbol, type);
      await RespondAsync(result.Message);
    }

    [SlashCommand("add", "Add an asset to an account.")]
    public async Task AddAsync(
        [Summary("account"), Autocomplete(typeof(AccountAutocompleteHandler))]
            int accountId, string symbol, decimal amount, decimal cost
    )
    {

      var result = await _assetService.AddStockAsync((long)Context.User.Id, accountId, symbol, amount, cost);
      await RespondAsync(result.Message);
    }

    [SlashCommand("portfolio", "Get value of asset portfolio.")]
    public async Task PortfolioAsync()
    {
      var result = await _assetService.GetPortfolioAsync((long)Context.User.Id);
      await RespondAsync(result.Message);
    }

    // TODO get value of a specific account instead of entire portfolio
  }
}
