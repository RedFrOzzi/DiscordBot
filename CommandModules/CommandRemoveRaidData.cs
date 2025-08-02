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
            bool isAuthorized = await IsAuthorized();
            if (!isAuthorized) { return; }

            if (string.IsNullOrEmpty(userName))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Пльзователь не указан"
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            if (!RaidFilesLoader.TryLoadRaidData(out var data))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Файл рейда не создан, сперва используй /создать"
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
                    Content = "Участник не найден"
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            if (!RaidFilesLoader.TrySaveRaidData(data!))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Участник не обновлен, файл не записан"
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            InteractionMessageProperties successProps = new()
            {
                Content = $"Участник <{userName}> удален."
            };
            var successMsg = InteractionCallback.Message(successProps);

            await RespondAsync(successMsg);
            return;
        }

        [SubSlashCommand("участник", "Добавить участника в рейд. Число указывает количество рейдов")]
        public async Task RemoveUserFrom(
            [SlashCommandParameter(Name = "участник")] GuildUser user)
        {
            bool isAuthorized = await IsAuthorized();
            if (!isAuthorized) { return; }

            if (string.IsNullOrEmpty(user.Username))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Пльзователь не указан или его имя не существует"
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            if (!RaidFilesLoader.TryLoadRaidData(out var data))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Файл рейда не создан, сперва используй /создать"
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
                    Content = "Участник не найден"
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            if (!RaidFilesLoader.TrySaveRaidData(data!))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Участник не обновлен, файл не записан"
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            InteractionMessageProperties successProps = new()
            {
                Content = $"Участник <{user}> удален."
            };
            var successMsg = InteractionCallback.Message(successProps);

            await RespondAsync(successMsg);
            return;
        }



        private async Task<bool> IsAuthorized()
        {
            var guildUser = Context.User as GuildUser;

            if (guildUser == null || Context.Guild == null)
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Ошибка пользователя"
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return false;
            }

            var roles = guildUser.GetRoles(Context.Guild);
            bool isAllowed = false;
            foreach (var role in roles)
            {
                if (BotApplicationSettings.Instance.HasRole(role))
                {
                    isAllowed = true;
                    break;
                }
            }

            if (!isAllowed)
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Роль пользователя не авторизована"
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return false;
            }

            return true;
        }
    }
}
