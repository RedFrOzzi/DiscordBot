using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DiscordBot.CommandModules
{
    [SlashCommand("бот", "Утилитарные команды")]
    public class CommandUtilitiesModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        readonly RestClient _client;

        public CommandUtilitiesModule(RestClient client)
        {
            _client = client;
        }

        [SubSlashCommand("удалить_текст", "Удаляет все текстовые сообщения бота, кроме тех, что содержат контент.")]
        public async Task DeleteAllBotsMessagesExcludingContent()
        {
            try
            {
                await _client.DeleteMessagesAsync(Context.Channel.Id, GetBotsTextMessages(_client, Context));
            }
            catch
            {
                await RespondAsync(InteractionCallback.Message(new() { Content = "Ошибка, сообщения не удалены или удалены частично" }));
                return;
            }

            await RespondAsync(InteractionCallback.Message(new() { Content = "Текстовые сообщения успешно удалены" }));
        }

        [SubSlashCommand("удалить_все", "Удаляет все сообщения бота")]
        public async Task DeleteAllBotsMessages()
        {
            try
            {
                await _client.DeleteMessagesAsync(Context.Channel.Id, GetBotsMessages(_client, Context));
            }
            catch
            {
                await RespondAsync(InteractionCallback.Message(new() { Content = "Ошибка, сообщения не удалены или удалены частично" }));
                return;
            }

            await RespondAsync(InteractionCallback.Message(new() { Content = "Текстовые сообщения успешно удалены" }));
        }

        [SubSlashCommand("удалить_сообщения_юзера", "Удаляет все сообщения выбранного пользователя")]
        public async Task DeleteAllBotsMessages(
            [SlashCommandParameter(Name = "пользователь", Description = "пользователь, чьи сообщения будут удалены")] GuildUser user)
        {
            try
            {
                await _client.DeleteMessagesAsync(Context.Channel.Id, GetUserMessages(_client, Context, user.Id));
            }
            catch
            {
                await RespondAsync(InteractionCallback.Message(new() { Content = "Ошибка, сообщения не удалены или удалены частично" }));
                return;
            }

            await RespondAsync(InteractionCallback.Message(new() { Content = "Текстовые сообщения успешно удалены" }));
        }

        private static async IAsyncEnumerable<ulong> GetBotsTextMessages(RestClient client, ApplicationCommandContext context)
        {
            await foreach (var msg in client.GetMessagesAsync(context.Channel.Id))
            {
                if (msg == null || !msg.Author.IsBot) { continue; }
                if (msg.Components.Any()) { continue; }
                if (msg.Embeds.Any()) { continue; }

                yield return msg.Id;
            }
        }

        private static async IAsyncEnumerable<ulong> GetBotsMessages(RestClient client, ApplicationCommandContext context)
        {
            await foreach (var msg in client.GetMessagesAsync(context.Channel.Id))
            {
                if (msg == null || !msg.Author.IsBot) { continue; }

                yield return msg.Id;
            }
        }

        private static async IAsyncEnumerable<ulong> GetUserMessages(RestClient client, ApplicationCommandContext context, ulong userId)
        {
            await foreach (var msg in client.GetMessagesAsync(context.Channel.Id))
            {
                if (msg == null || msg.Author.Id != userId) { continue; }

                yield return msg.Id;
            }
        }
    }
}
