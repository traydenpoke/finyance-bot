using Discord.Interactions;
using FinyanceApp.Database;

namespace FinyanceApp.Commands
{
  [Group("asset", "Manage assets.")]
  public class AssetCommands : InteractionModuleBase<SocketInteractionContext>
  {
    private readonly AssetService _assetService;

    public AssetCommands(AssetService assetService)
    {
      _assetService = assetService;
    }

    public enum AssetType { Stock, Crypto }

    [SlashCommand("price", "Get current price of an asset.")]
    public async Task PriceAsync(string symbol, AssetType type)
    {
      var result = await _assetService.GetPriceAsync(symbol, type);
      await RespondAsync(result.Message);
    }
  }
}
