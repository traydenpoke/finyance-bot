using FinyanceApp.Models.Interaces;
using Npgsql;

namespace FinyanceApp.Models
{
  public class Account : IDbModel
  {
    public int Id { get; set; }
    public long UserId { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public decimal Balance { get; set; }


    public void LoadFromReader(NpgsqlDataReader reader)
    {
      Id = reader.GetInt32(reader.GetOrdinal("id"));
      UserId = reader.GetInt64(reader.GetOrdinal("user_id"));
      Description = reader.GetString(reader.GetOrdinal("description"));
      Type = reader.GetString(reader.GetOrdinal("type"));
      Balance = reader.GetDecimal(reader.GetOrdinal("balance"));
    }

    public (string Columns, string Parameters, List<NpgsqlParameter> Values) GetInsertDef()
    {
      var columns = "user_id, description, type, balance";
      var parameters = "@user_id, @description, @type, @balance";

      var values = new List<NpgsqlParameter>
      {
        new("user_id", UserId),
        new("description", Description),
        new("type", Type),
        new("balance", Balance)
      };

      return (columns, parameters, values);
    }

    public void SetId(object id)
    {
      Id = Convert.ToInt32(id);
    }

    public string ToString()
    {
      return $"ID: {Id} Type: {Type} Description: {Description} Balance: {Balance}";
    }
  }
}