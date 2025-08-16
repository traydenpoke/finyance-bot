using Discord.Interactions;
using FinyanceApp.Database;
using FinyanceApp.Models.Enums;

namespace FinyanceApp.Commands
{
  [Group("account", "Manage accounts.")]
  public class AccountCommands : InteractionModuleBase<SocketInteractionContext>
  {
    private readonly AccountService _accountService;

    public AccountCommands(AccountService accountService)
    {
      _accountService = accountService;
    }

    [SlashCommand("create", "Create an account")]
    public async Task CreateAsync(string description, AccountType type, decimal balance = 0)
    {
      var result = await _accountService.CreateAccountAsync((long)Context.User.Id, description, type, balance);
      await RespondAsync(result.Message);
    }

    [SlashCommand("delete", "Delete an account")]
    public async Task DeleteAsync(int accountId)
    {
      var result = await _accountService.DeleteAccountAsync((long)Context.User.Id, accountId);
      await RespondAsync(result.Message);
    }

    [SlashCommand("get", "Get account by id.")]
    public async Task GetAsync(int accountId)
    {
      var result = await _accountService.GetAccountAsync((long)Context.User.Id, accountId);
      await RespondAsync(result.Message);
    }

    [SlashCommand("gets", "Get all my accounts.")]
    public async Task GetsAsync()
    {
      var result = await _accountService.GetUserAccountsAsync((long)Context.User.Id);
      await RespondAsync(result.Message);
    }

    [SlashCommand("update", "Update money in account (+/-)")]
    public async Task UpdateAsync(int accountId, decimal amount)
    {
      var result = await _accountService.UpdateBalanceAsync((long)Context.User.Id, accountId, amount);
      await RespondAsync(result.Message);
    }

    [SlashCommand("set", "Set balance for an account")]
    public async Task SetAsync(int accountId, decimal balance)
    {
      var result = await _accountService.SetBalanceAsync((long)Context.User.Id, accountId, balance);
      await RespondAsync(result.Message);
    }
  }

}
