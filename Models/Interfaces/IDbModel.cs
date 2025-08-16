using Npgsql;

namespace FinyanceApp.Models.Interaces
{
  public interface IDbModel
  {
    void LoadFromReader(NpgsqlDataReader reader);

    (string Columns, string Parameters, List<NpgsqlParameter> Values) GetInsertDef();

    void SetId(object id);

    string ToString();
  }
}
