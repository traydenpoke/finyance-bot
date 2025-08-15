# Finyance Discord bot

## To run

- `git clone https://github.com/traydenpoke/finyance-bot.git`
- `cd finyance-bot`
- `dotnet restore`
- create `appsettings.json` in root dir and include fields `Discord.Token` and `Database.ConnectionString`
- `Discord.Token`: access token from Discord dev portal ([Link](https://discord.com/developers/applications))
- `Database.ConnectionString`: Postgres connection string; `(Host=A;Database=B;Username=C;Password=D;)`
- `dotnet run`
