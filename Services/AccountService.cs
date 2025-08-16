using FinyanceApp.Data;
using FinyanceApp.Models;
using FinyanceApp.Models.Enums;
using FinyanceApp.Results;

namespace FinyanceApp.Database
{

  public class AccountService
  {
    private readonly PostgresService _postgres;

    public AccountService(PostgresService postgres)
    {
      _postgres = postgres;
    }

    public async Task<ServiceResult<Account>> CreateAccountAsync(long userId, string description, AccountType type, decimal balance)
    {
      var typeString = EnumToDbValue(type);

      var existing = await _postgres.GetByFieldsAsync<Account>(
          TableName.Accounts,
          new Dictionary<string, object>
          {
            { "user_id", userId },
            { "description", description },
            { "type", typeString }
          }
      );

      if (existing.Any())
        return ServiceResult<Account>.Fail($"{type} account already exists with this description!");

      var newAccount = new Account
      {
        UserId = userId,
        Description = description,
        Type = typeString,
        Balance = balance
      };

      await _postgres.AddAsync(TableName.Accounts, newAccount);
      return ServiceResult<Account>.Ok(newAccount, $"Created new account with id {newAccount.Id}");
    }

    public async Task<ServiceResult<bool>> DeleteAccountAsync(long userId, int accountId)
    {
      if (!await UserOwnsAccount(userId, accountId))
        return ServiceResult<bool>.Fail("You do not own this account.");

      bool deleted = await _postgres.DeleteByIdAsync(TableName.Accounts, accountId);
      return deleted
          ? ServiceResult<bool>.Ok(true, "Deleted.")
          : ServiceResult<bool>.Fail("Failed to delete.");
    }

    public async Task<ServiceResult<Account>> GetAccountAsync(long userId, int accountId)
    {
      var account = await GetAccountRecord(userId, accountId);
      return account != null
          ? ServiceResult<Account>.Ok(account, account.ToString())
          : ServiceResult<Account>.Fail("Not found.");
    }

    public async Task<ServiceResult<List<Account>>> GetUserAccountsAsync(long userId)
    {
      var fields = new Dictionary<string, object> { { "user_id", userId } };
      var accounts = await _postgres.GetByFieldsAsync<Account>(TableName.Accounts, fields);

      return accounts.Any()
          ? ServiceResult<List<Account>>.Ok(accounts, $"{accounts.Count} accounts found.")
          : ServiceResult<List<Account>>.Fail("No accounts found.");
    }

    public async Task<ServiceResult<bool>> UpdateBalanceAsync(long userId, int accountId, decimal amount)
    {
      var account = await GetAccountRecord(userId, accountId);
      if (account == null) return ServiceResult<bool>.Fail("Account not found.");

      var newBalance = account.Balance + amount;
      if (newBalance < 0) return ServiceResult<bool>.Fail("Insufficient funds.");

      bool updated = await _postgres.UpdateFieldsByIdAsync(
          TableName.Accounts, accountId,
          new Dictionary<string, object> { { "balance", newBalance } }
      );

      return updated
          ? ServiceResult<bool>.Ok(true, "Updated balance.")
          : ServiceResult<bool>.Fail("Failed to update balance.");
    }

    public async Task<ServiceResult<bool>> SetBalanceAsync(long userId, int accountId, decimal balance)
    {
      if (balance < 0) return ServiceResult<bool>.Fail("Balance cannot be negative.");
      if (!await UserOwnsAccount(userId, accountId)) return ServiceResult<bool>.Fail("You do not own this account.");

      bool updated = await _postgres.UpdateFieldsByIdAsync(
          TableName.Accounts, accountId,
          new Dictionary<string, object> { { "balance", balance } }
      );

      return updated
          ? ServiceResult<bool>.Ok(true, "Set balance.")
          : ServiceResult<bool>.Fail("Failed to set balance.");
    }

    // --- Private helpers ---
    private async Task<Account?> GetAccountRecord(long userId, int accountId)
    {
      var accounts = await _postgres.GetByFieldsAsync<Account>(
          TableName.Accounts,
          new Dictionary<string, object>
          {
                    { "id", accountId },
                    { "user_id", userId }
          }
      );
      return accounts.FirstOrDefault();
    }

    private async Task<bool> UserOwnsAccount(long userId, int accountId) =>
        await GetAccountRecord(userId, accountId) != null;

    private static string EnumToDbValue(AccountType type) =>
        type.ToString().ToLower(); // Centralized enum-to-string conversion
  }
}
