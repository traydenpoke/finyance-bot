using FinyanceApp.Commands;
using FinyanceApp.Models;

namespace FinyanceApp.Database
{
  public class AccountServiceResult
  {
    public bool Success { get; set; }
    public string Message { get; set; } = "";
  }

  public class AccountService
  {
    private readonly PostgresService _postgres;

    private static AccountServiceResult Ok(string msg) => new() { Success = true, Message = msg };
    private static AccountServiceResult Fail(string msg) => new() { Success = false, Message = msg };

    public AccountService(PostgresService postgres)
    {
      _postgres = postgres;
    }

    public async Task<AccountServiceResult> CreateAccountAsync(long userId, string description, AccountCommands.AccountType type, decimal balance)
    {
      var existing = await _postgres.GetByFieldsAsync<Account>(
          PostgresService.TableName.Accounts,
          new Dictionary<string, object> { { "user_id", userId }, { "description", description }, { "type", type.ToString().ToLower() } }
      );

      if (existing.Any())
        return Fail($"{type} account already exists with this description!");

      var newAccount = new Account
      {
        UserId = userId,
        Description = description,
        Type = type.ToString().ToLower(),
        Balance = balance
      };

      await _postgres.AddAsync(PostgresService.TableName.Accounts, newAccount);
      return Ok($"Created new account with id {newAccount.Id}");
    }

    public async Task<AccountServiceResult> DeleteAccountAsync(long userId, int accountId)
    {
      if (!await UserOwnsAccount(userId, accountId))
        return Fail("You do not own this account.");

      bool deleted = await _postgres.DeleteByIdAsync(PostgresService.TableName.Accounts, accountId);
      return deleted ? Ok("Deleted.") : Fail("Failed to delete.");
    }

    public async Task<AccountServiceResult> GetAccountAsync(long userId, int accountId)
    {
      var account = await _postgres.GetByFieldsAsync<Account>(
          PostgresService.TableName.Accounts,
          new Dictionary<string, object> { { "id", accountId }, { "user_id", userId } }
      );

      return account.FirstOrDefault() is { } a
          ? Ok(a.ToString())
          : Fail("Not found.");
    }

    public async Task<AccountServiceResult> GetUserAccountsAsync(long userId)
    {
      var accounts = await _postgres.GetByFieldsAsync<Account>(
          PostgresService.TableName.Accounts,
          new Dictionary<string, object> { { "user_id", userId } }
      );

      if (!accounts.Any()) return Fail("No accounts found.");
      var output = string.Join("\n", accounts.Select(a => a.ToString()));
      return Ok(output);
    }

    public async Task<AccountServiceResult> UpdateBalanceAsync(long userId, int accountId, decimal amount)
    {
      var account = await GetAccountRecord(userId, accountId);
      if (account == null) return Fail("Account not found.");

      var newBalance = account.Balance + amount;
      if (newBalance < 0) return Fail("Insufficient funds.");

      bool updated = await _postgres.UpdateFieldsByIdAsync(
          PostgresService.TableName.Accounts, accountId,
          new Dictionary<string, object> { { "balance", newBalance } }
      );

      return updated ? Ok("Updated balance.") : Fail("Failed to update balance.");
    }

    public async Task<AccountServiceResult> SetBalanceAsync(long userId, int accountId, decimal balance)
    {
      if (balance < 0) return Fail("Balance cannot be negative.");
      if (!await UserOwnsAccount(userId, accountId)) return Fail("You do not own this account.");

      bool updated = await _postgres.UpdateFieldsByIdAsync(
          PostgresService.TableName.Accounts, accountId,
          new Dictionary<string, object> { { "balance", balance } }
      );

      return updated ? Ok("Set balance.") : Fail("Failed to set balance.");
    }

    // Private helpers
    private async Task<Account?> GetAccountRecord(long userId, int accountId)
    {
      var accounts = await _postgres.GetByFieldsAsync<Account>(
          PostgresService.TableName.Accounts,
          new Dictionary<string, object> { { "id", accountId }, { "user_id", userId } }
      );
      return accounts.FirstOrDefault();
    }

    private async Task<bool> UserOwnsAccount(long userId, int accountId) =>
        await GetAccountRecord(userId, accountId) != null;
  }

}