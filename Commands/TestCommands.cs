using Discord;
using Discord.Interactions;

namespace FinyanceApp.Commands
{
  public class TestCommands : InteractionModuleBase<SocketInteractionContext>
  {
    [SlashCommand("nya", "Replies with nya!")]
    public async Task NyaAsync()
    {
      await RespondAsync("nya!");
    }

    [SlashCommand("clear", "clear chat.")]
    public async Task ClearAsync()
    {
      var channel = Context.Channel as ITextChannel;
      var messages = await channel.GetMessagesAsync(100).FlattenAsync();
      await channel.DeleteMessagesAsync(messages);
      await RespondAsync("done!", ephemeral: true);
    }
  }
}
