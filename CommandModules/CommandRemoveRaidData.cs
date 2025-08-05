using DiscordBot.Models;
using DiscordBot.Utilities;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Xml.Linq;

namespace DiscordBot.CommandModules
{
    [SlashCommand("убрать", "Убрать рейд или участника")]
    public class CommandRemoveRaidData : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("имя_участника", "Добавить участника в рейд. Число указывает количество рейдов")]
        public async Task RemoveUserNameFromRaid(
            [SlashCommandParameter(Name = "имя")] string userName)
        {
            if (!await this.IsAuthorized()) { return; }

            if (string.IsNullOrEmpty(userName))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Пльзователь не указан",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            if (!RaidFilesLoader.TryLoadRaidData(out var data))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Файл рейда не создан, сперва используй /создать",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            bool isFound = false;
            for (int i = 0; i < data!.RaidUsers.Count; i++)
            {
                if (data!.RaidUsers[i] != null && data!.RaidUsers[i].Name == userName)
                {
                    isFound = true;
                    data!.RaidUsers.RemoveAt(i);
                    break;
                }
            }

            if (!isFound)
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Участник не найден",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            if (!RaidFilesLoader.TrySaveRaidData(data!))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Участник не обновлен, файл не записан",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            InteractionMessageProperties successProps = new()
            {
                Content = $"Участник <{userName}> удален.",
                Flags = MessageFlags.Ephemeral
            };
            var successMsg = InteractionCallback.Message(successProps);

            await RespondAsync(successMsg);
            return;
        }

        [SubSlashCommand("участник", "Добавить участника в рейд. Число указывает количество рейдов")]
        public async Task RemoveUserFrom(
            [SlashCommandParameter(Name = "участник")] GuildUser user)
        {
            if (!await this.IsAuthorized()) { return; }

            if (string.IsNullOrEmpty(user.Username))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Пльзователь не указан или его имя не существует",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            if (!RaidFilesLoader.TryLoadRaidData(out var data))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Файл рейда не создан, сперва используй /создать",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            bool isFound = false;
            for (int i = 0; i < data!.RaidUsers.Count; i++)
            {
                if (data!.RaidUsers[i] != null && data!.RaidUsers[i].Name == user.Username)
                {
                    isFound = true;
                    data!.RaidUsers.RemoveAt(i);
                    break;
                }
            }

            if (!isFound)
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Участник не найден",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            if (!RaidFilesLoader.TrySaveRaidData(data!))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Участник не обновлен, файл не записан",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            InteractionMessageProperties successProps = new()
            {
                Content = $"Участник <{user}> удален.",
                Flags = MessageFlags.Ephemeral
            };
            var successMsg = InteractionCallback.Message(successProps);

            await RespondAsync(successMsg);
            return;
        }
    }
}
