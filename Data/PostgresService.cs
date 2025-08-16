using System.Text;
using FinyanceApp.Models.Enums;
using FinyanceApp.Models.Interaces;
using Npgsql;

namespace FinyanceApp.Data
{
  public class PostgresService
  {
    private readonly DatabaseManager _db;
    public PostgresService(DatabaseManager db)
    {
      _db = db;
    }

    // Helper to create a typed parameter
    private NpgsqlParameter GenerateParameter(string name, object value)
    {
      return new NpgsqlParameter(name, value switch
      {
        int => NpgsqlTypes.NpgsqlDbType.Integer,
        long => NpgsqlTypes.NpgsqlDbType.Bigint,
        string => NpgsqlTypes.NpgsqlDbType.Text,
        float => NpgsqlTypes.NpgsqlDbType.Real,
        double => NpgsqlTypes.NpgsqlDbType.Double,
        decimal => NpgsqlTypes.NpgsqlDbType.Numeric,
        bool => NpgsqlTypes.NpgsqlDbType.Boolean,
        _ => NpgsqlTypes.NpgsqlDbType.Text
      })
      {
        Value = value
      };
    }

    // Get all rows of a table
    public async Task<List<T>> GetAsync<T>(TableName tableName) where T : IDbModel, new()
    {
      var dict = new Dictionary<string, object>();
      return await GetByFieldsAsync<T>(tableName, dict);
    }

    // Get all rows of a table by filter
    public async Task<List<T>> GetByFieldsAsync<T>(
      TableName tableName,
      Dictionary<string, object> fields
    ) where T : IDbModel, new()
    {
      var sql = new StringBuilder($"SELECT * FROM {tableName}");
      var parameters = new List<NpgsqlParameter>();

      if (fields != null && fields.Count > 0)
      {
        sql.Append(" WHERE ");
        var conditions = new List<string>();

        foreach (var kvp in fields)
        {
          var paramName = $"@{kvp.Key}";
          conditions.Add($"{kvp.Key} = {paramName}");
          parameters.Add(GenerateParameter(kvp.Key, kvp.Value));
        }

        sql.Append(string.Join(" AND ", conditions));
      }

      var result = new List<T>();
      await using var conn = _db.GetConnection();
      await conn.OpenAsync();

      await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
      cmd.Parameters.AddRange(parameters.ToArray());

      await using var reader = await cmd.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        var instance = new T();
        instance.LoadFromReader(reader);
        result.Add(instance);
      }

      return result;
    }

    // Insert to a table
    public async Task AddAsync<T>(TableName tableName, T model) where T : IDbModel
    {
      var (columns, parameters, values) = model.GetInsertDef();
      var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters}) RETURNING id";

      await using var conn = _db.GetConnection();
      await conn.OpenAsync();

      await using var cmd = new NpgsqlCommand(sql, conn);
      cmd.Parameters.AddRange(values.Select(v => GenerateParameter(v.ParameterName, v.Value)).ToArray());

      var result = await cmd.ExecuteScalarAsync();
      model.SetId(result);
    }

    // Delete from a table by id
    public async Task<bool> DeleteByIdAsync(TableName tableName, int id)
    {
      var sql = $"DELETE FROM {tableName} WHERE id = @id";

      await using var conn = _db.GetConnection();
      await conn.OpenAsync();

      await using var cmd = new NpgsqlCommand(sql, conn);
      cmd.Parameters.Add(GenerateParameter("id", id));

      int rowsAffected = await cmd.ExecuteNonQueryAsync();
      return rowsAffected > 0;
    }

    // Update row fields by id
    public async Task<bool> UpdateFieldsByIdAsync(
        TableName tableName,
        int id,
        Dictionary<string, object> fields
    )
    {
      if (fields == null || fields.Count == 0)
        throw new ArgumentException("No fields provided for update", nameof(fields));

      var sql = new StringBuilder($"UPDATE {tableName} SET ");
      var parameters = new List<NpgsqlParameter>();

      int counter = 0;
      foreach (var kvp in fields)
      {
        if (counter > 0) sql.Append(", ");
        sql.Append($"{kvp.Key} = @{kvp.Key}");
        parameters.Add(GenerateParameter(kvp.Key, kvp.Value));
        counter++;
      }

      sql.Append(" WHERE id = @id");
      parameters.Add(GenerateParameter("id", id));

      await using var conn = _db.GetConnection();
      await conn.OpenAsync();

      await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
      cmd.Parameters.AddRange(parameters.ToArray());

      int rowsAffected = await cmd.ExecuteNonQueryAsync();
      return rowsAffected > 0;
    }
  }
}
