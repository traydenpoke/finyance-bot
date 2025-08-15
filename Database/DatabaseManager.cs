using Npgsql;

namespace FinyanceApp.Database
{
  public class DatabaseManager
  {
    private readonly string _connectionString;

    public DatabaseManager(string connectionString)
    {
      _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
      await CreateDatabaseIfNotExistsAsync();
      await CreateTablesIfNotExistsAsync();
    }

    private async Task CreateDatabaseIfNotExistsAsync()
    {
      var builder = new NpgsqlConnectionStringBuilder(_connectionString);
      string dbName = builder.Database;

      // Connect to postgres default db to check/create your bot db
      builder.Database = "postgres";

      using var conn = new NpgsqlConnection(builder.ConnectionString);
      await conn.OpenAsync();

      using var cmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{dbName}'", conn);
      var exists = await cmd.ExecuteScalarAsync();

      if (exists == null)
      {
        using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", conn);
        await createCmd.ExecuteNonQueryAsync();
      }
    }

    private async Task CreateTable(string tableSql)
    {
      await using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();

      await using var cmd = new NpgsqlCommand(tableSql, conn);
      await cmd.ExecuteNonQueryAsync();
    }

    private async Task CreateTablesIfNotExistsAsync()
    {
      using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync();


      await CreateTable(@"
        CREATE TABLE IF NOT EXISTS accounts (
          id SERIAL PRIMARY KEY,
          user_id BIGINT NOT NULL,
          description TEXT NOT NULL,
          type TEXT CHECK(type IN ('cash', 'stock', 'crypto')) NOT NULL,
          balance NUMERIC NOT NULL,
          UNIQUE(user_id, description, type)
        );
      ");

      await CreateTable(@"
        CREATE TABLE IF NOT EXISTS assets (
          id SERIAL PRIMARY KEY,
          account_id INTEGER REFERENCES accounts(id),
          symbol TEXT NOT NULL,
          description TEXT NOT NULL,
          type TEXT CHECK(type IN ('stock', 'crypto')) NOT NULL
        );
      ");
    }

    public NpgsqlConnection GetConnection()
    {
      return new NpgsqlConnection(_connectionString);
    }
  }
}