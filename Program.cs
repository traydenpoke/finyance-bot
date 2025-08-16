using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FinyanceApp.Services;
using FinyanceApp.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


using System.Reflection;
using FinyanceApp.Data;

namespace FinyanceApp
{
  internal class Program
  {
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private string _token;
    private string _connectionString;

    public Program()
    {
      // Load env vaiables
      _config = new ConfigurationBuilder()
          .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"))
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables()
          .Build();

      _token = _config["Discord:Token"]
                     ?? throw new Exception("Discord token not set in configuration.");
      _connectionString = _config["Database:ConnectionString"]
                                ?? throw new Exception("Database connection string not set.");

      // Discord client
      var config = new DiscordSocketConfig
      {
        GatewayIntents = GatewayIntents.AllUnprivileged,
        UseInteractionSnowflakeDate = false
      };
      _client = new DiscordSocketClient(config);
      _interactionService = new InteractionService(_client.Rest);

      // Build DI container
      _services = new ServiceCollection()
          .AddSingleton(_client)
          .AddSingleton(_interactionService)
          .AddSingleton(_config)
          .AddSingleton<GoogleFinanceService>()
          .AddSingleton<DatabaseManager>(provider => new DatabaseManager(_connectionString))
          .AddSingleton<PostgresService>()
          .AddSingleton<AccountService>()
          .AddSingleton<AssetService>()
          .BuildServiceProvider();

      // Discord events
      _client.Log += LogAsync;
      _client.Ready += ClientReadyAsync;
      _client.InteractionCreated += HandleInteractionAsync;
    }

    public async Task StartBotAsync()
    {
      // Initialize database
      var dbManager = _services.GetRequiredService<DatabaseManager>();
      await dbManager.InitializeAsync();

      // Start bot
      await _client.LoginAsync(TokenType.Bot, _token);
      await _client.StartAsync();
      await Task.Delay(-1);
    }

    private async Task ClientReadyAsync()
    {
      await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
      await _interactionService.RegisterCommandsToGuildAsync(696154247326072834);
      Console.WriteLine("Bot ready and slash commands registered.");
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
      try
      {
        var ctx = new SocketInteractionContext(_client, interaction);
        await _interactionService.ExecuteCommandAsync(ctx, _services);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        if (interaction.Type == InteractionType.ApplicationCommand)
        {
          await interaction.GetOriginalResponseAsync()
              .ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
      }
    }

    private Task LogAsync(LogMessage message)
    {
      Console.WriteLine(message.ToString());
      return Task.CompletedTask;
    }

    static void Main(string[] args) =>
        new Program().StartBotAsync().GetAwaiter().GetResult();
  }
}
