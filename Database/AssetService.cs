using FinyanceApp.Commands;
using FinyanceApp.Services;

namespace FinyanceApp.Database
{
  public class AssetServiceResult
  {
    public bool Success { get; set; }
    public string Message { get; set; } = "";
  }

  public class AssetService
  {
    private readonly PostgresService _postgres;
    private readonly GoogleFinanceService _financeService;

    private static AssetServiceResult Ok(string msg) => new() { Success = true, Message = msg };
    private static AssetServiceResult Fail(string msg) => new() { Success = false, Message = msg };

    public AssetService(PostgresService postgres, GoogleFinanceService financeService)
    {
      _postgres = postgres;
      _financeService = financeService;

    }

    public async Task<AssetServiceResult> GetPriceAsync(string symbol, AssetCommands.AssetType type)
    {
      var price = await _financeService.GetPriceAsync(symbol, type.ToString().ToLower());
      if (price.HasValue)
        return Ok($"${price.Value}");

      return Fail("Could not fetch price.");
    }
  }

}