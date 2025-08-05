using DiscordBot.Models;
using DiscordBot.Utilities;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DiscordBot.CommandModules
{
    public class CommandSettingsModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("доступ", "Настройка прав доступа к командам")]
        public async Task CreateSettings(Role role)
        {
            if (!await this.IsAuthorized()) { return; }

            if (BotApplicationSettings.Instance.HasRole(role))
            {
                InteractionMessageProperties errorMsgProp2 = new()
                {
                    Content = "Данная роль уже существует"
                };
                var errorMsg2 = InteractionCallback.Message(errorMsgProp2);

                await RespondAsync(errorMsg2);
                return;
            }

            BotApplicationSettings.Instance.ModerationRoles.Add(role.Id, role.Name);

            if (!RaidFilesLoader.TrySaveSettings(BotApplicationSettings.Instance))
            {
                //local changes rollback
                BotApplicationSettings.Instance.ModerationRoles.Remove(role.Id);

                InteractionMessageProperties errorMsgProp3 = new()
                {
                    Content = "Ошибка записи на диск"
                };
                var errorMsg3 = InteractionCallback.Message(errorMsgProp3);

                await RespondAsync(errorMsg3);
                return;
            }

            InteractionMessageProperties msgProps = new()
            {
                Content = "Роль успешно добавлена"
            };
            var msg = InteractionCallback.Message(msgProps);

            await RespondAsync(msg);
        }
    }
}
