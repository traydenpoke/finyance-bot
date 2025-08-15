using Npgsql;

namespace FinyanceApp.Models
{
  public class Asset : IDbModel
  {
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Symbol { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }

    public void LoadFromReader(NpgsqlDataReader reader)
    {
      Id = reader.GetInt32(reader.GetOrdinal("id"));
      AccountId = reader.GetInt32(reader.GetOrdinal("account_id"));
      Symbol = reader.GetString(reader.GetOrdinal("symbol"));
      Description = reader.GetString(reader.GetOrdinal("description"));
      Type = reader.GetString(reader.GetOrdinal("type"));
    }

    public (string Columns, string Parameters, List<NpgsqlParameter> Values) GetInsertDef()
    {
      var columns = "account_id, symbol, description, type";
      var parameters = "@account_id, @symbol, @description, @type";

      var values = new List<NpgsqlParameter>
      {
        new("account_id", AccountId),
        new("symbol", Symbol),
        new("description", Description),
        new("type", Type)
      };

      return (columns, parameters, values);
    }

    public void SetId(object id)
    {
      Id = Convert.ToInt32(id);
    }
  }
}