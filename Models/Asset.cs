using FinyanceApp.Models.Interaces;
using Npgsql;

namespace FinyanceApp.Models
{
  public class Asset : IDbModel
  {
    public int Id { get; set; }
    public int AccountId { get; set; }
    public long UserId { get; set; }
    public string Symbol { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Cost { get; set; }

    public void LoadFromReader(NpgsqlDataReader reader)
    {
      Id = reader.GetInt32(reader.GetOrdinal("id"));
      AccountId = reader.GetInt32(reader.GetOrdinal("account_id"));
      UserId = reader.GetInt64(reader.GetOrdinal("user_id"));
      Symbol = reader.GetString(reader.GetOrdinal("symbol"));
      Description = reader.GetString(reader.GetOrdinal("description"));
      Type = reader.GetString(reader.GetOrdinal("type"));
      Amount = reader.GetDecimal(reader.GetOrdinal("amount"));
      Cost = reader.GetDecimal(reader.GetOrdinal("cost"));
    }

    public (string Columns, string Parameters, List<NpgsqlParameter> Values) GetInsertDef()
    {
      var columns = "account_id, user_id, symbol, description, type, amount, cost";
      var parameters = "@account_id, @user_id, @symbol, @description, @type, @amount, @cost";

      var values = new List<NpgsqlParameter>
      {
        new("account_id", AccountId),
        new("user_id", UserId),
        new("symbol", Symbol),
        new("description", Description),
        new("type", Type),
        new("amount", Amount),
        new("cost", Cost)
      };

      return (columns, parameters, values);
    }

    public void SetId(object id)
    {
      Id = Convert.ToInt32(id);
    }
  }
}