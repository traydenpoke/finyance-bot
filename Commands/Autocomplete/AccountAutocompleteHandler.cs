using Discord;
using Discord.Interactions;
using FinyanceApp.Database;

namespace FinyanceApp.Commands.Autocomplete
{
  public class AccountAutocompleteHandler : AutocompleteHandler
  {
    private readonly AccountService _accountService;

    public AccountAutocompleteHandler(AccountService accountService)
    {
      _accountService = accountService;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services
    )
    {
      var results = new List<AutocompleteResult>();
      var result = await _accountService.GetUserAccountsAsync((long)context.User.Id);
      var accounts = result.Data ??= [];

      foreach (var account in accounts)
      {
        results.Add(new AutocompleteResult($"{account.Type} - {account.Description} ({account.Id})", account.Id.ToString()));
      }

      // max 25 suggestions for Discord API
      return AutocompletionResult.FromSuccess(results.Take(25));
    }
  }
}