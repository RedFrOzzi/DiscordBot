using DiscordBot.Models;
using DiscordBot.Utilities;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DiscordBot.CommandModules
{
    [SlashCommand("добавить", "Добавить рейд или участника")]
    public class CommandAddRaidDataModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("участника", "Добавить участника в рейд. Число указывает количество рейдов")]
        public async Task AddUserAuto(
            [SlashCommandParameter(Name = "участник")] GuildUser guildUser,
            [SlashCommandParameter(Name = "количество", Description = "количество добавится к текущему количеству")] int raidsCount)
        {
            if (!await this.IsAuthorized()) { return; }

            if (guildUser == null || string.IsNullOrEmpty(guildUser.Username) || raidsCount == 0)
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Пльзователь не указан или количество рейдов равно 0",
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

            //If user already in list
            for (int i = 0; i < data!.RaidUsers.Count; i++)
            {
                SerializedDiscordUser? raider = data!.RaidUsers[i];
                if (raider != null && raider.Name == guildUser.Username)
                {
                    raider.RaidsCount += raidsCount;

                    await TryAddUser(data!, raider.Name, raider.RaidsCount);
                    return;
                }
            }

            //If user was not in list
            var nickname = guildUser?.Nickname;
            var globalName = guildUser?.GlobalName;
            SerializedDiscordUser raidUser = new(guildUser!.Id, guildUser.Username, raidsCount)
            {
                NickName = nickname ?? string.Empty,
                GlobaName = globalName ?? string.Empty
            };
            data!.RaidUsers.Add(raidUser);
            await TryAddUser(data!, guildUser.Username, raidsCount);
        }

        [SubSlashCommand("рейд", "Добавить рейд")]
        public async Task AddRaid(
            [SlashCommandParameter(Name = "канал", Description = "канал в котором находятся участники рейда")] VoiceGuildChannel channel)
        {
            if (!await this.IsAuthorized()) { return; }

            var voiceUsers = Context?.Guild?.VoiceStates;

            //Check voice channels
            if (voiceUsers == null || voiceUsers.Count == 0)
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Голосовые каналы не имеют активных пользователей",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            //Check local file
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

            int channelUserCount = 0;
            foreach (var user in voiceUsers)
            {
                if (user.Value?.ChannelId == channel.Id)
                {
                    channelUserCount++;
                    var name = user.Value?.User?.Username;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    if (IsAlreadyInFile(data!.RaidUsers, name, out var foundUser))
                    {
                        foundUser!.RaidsCount++;
                        continue;
                    }

                    var nickname = user.Value?.User?.Nickname;
                    var globalName = user.Value?.User?.GlobalName;
                    SerializedDiscordUser raidUser = new(user.Key, name, 1)
                    {
                        NickName = nickname ?? string.Empty,
                        GlobaName = globalName ?? string.Empty
                    };

                    data.RaidUsers.Add(raidUser);
                }
            }

            //If channel users count is zero then abort action
            if (channelUserCount == 0)
            {
                InteractionMessageProperties zeroUsersMsgProps = new()
                {
                    Content = "Указанный голосовой канал не имеет активных пользователей",
                    Flags = MessageFlags.Ephemeral
                };
                var zeroUsersMsg = InteractionCallback.Message(zeroUsersMsgProps);

                await RespondAsync(zeroUsersMsg);
                return;
            }

            var embedProperties = new EmbedFieldProperties[data!.RaidUsers.Count];
            int totalUserRaidsCount = TotalUserRaidsCount(data.RaidUsers);

            for (int i = 0; i < data.RaidUsers.Count; i++)
            {
                if (string.IsNullOrEmpty(data.RaidUsers[i].NickName))
                {
                    if (string.IsNullOrEmpty(data.RaidUsers[i].GlobaName))
                    {
                        embedProperties[i] = new()
                        {
                            Name = $"{data.RaidUsers[i].Name}",
                            Value = $"Количество рейдов: {data.RaidUsers[i].RaidsCount}\nМожет расчитывать на {GetUserRaidsCountPercent(data.RaidUsers[i], totalUserRaidsCount)}% добычи."
                        };
                        continue;
                    }
                    else
                    {
                        embedProperties[i] = new()
                        {
                            Name = $"{data.RaidUsers[i].Name} ({data.RaidUsers[i].GlobaName})",
                            Value = $"Количество рейдов: {data.RaidUsers[i].RaidsCount}\nМожет расчитывать на {GetUserRaidsCountPercent(data.RaidUsers[i], totalUserRaidsCount)}% добычи."
                        };
                        continue;
                    }
                }

                embedProperties[i] = new()
                {
                    Name = $"{data.RaidUsers[i].Name} ({data.RaidUsers[i].NickName})",
                    Value = $"Количество рейдов: {data.RaidUsers[i].RaidsCount}\nМожет расчитывать на {GetUserRaidsCountPercent(data.RaidUsers[i], totalUserRaidsCount)}% добычи."
                };
            }

            EmbedProperties embed = new()
            {
                Title = "Рейд",
                Description = "Полный состав:",
                Fields = embedProperties
            };

            if (!RaidFilesLoader.TrySaveRaidData(data))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Ошибка записи файла",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await RespondAsync(errorMsg);
                return;
            }

            InteractionMessageProperties interactionMessageProperties = new()
            {
                Embeds = [embed]
            };
            var message = InteractionCallback.Message(interactionMessageProperties);
            await RespondAsync(message);
        }
        //---------------------------------------------------------------------------------------------------------------------------------

        //Info message
        private async Task TryAddUser(RaidData data, string name, int raidsCount)
        {
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
                Content = $"Участник <{name}> добавлен. Его итоговое участие: {raidsCount} раз(а).",
                Flags = MessageFlags.Ephemeral
            };
            var successMsg = InteractionCallback.Message(successProps);

            await RespondAsync(successMsg);
            return;
        }

        private static bool IsAlreadyInFile(List<SerializedDiscordUser> userList,
            string name,
            out SerializedDiscordUser? foundUser)
        {
            for (int i = 0; i < userList.Count; i++)
            {
                if (userList[i].Name == name)
                {
                    foundUser = userList[i];
                    return true;
                }
            }

            foundUser = null;
            return false;
        }

        private static int GetUserRaidsCountPercent(SerializedDiscordUser user, float totalUserRaids)
        {
            if (totalUserRaids < 1f) { return 0; }

            return (int)(((float)user.RaidsCount / totalUserRaids) * 100f);
        }

        private static int TotalUserRaidsCount(List<SerializedDiscordUser> userList)
        {
            int totalUserRaids = 0;
            for (int i = 0; i < userList.Count; i++)
            {
                totalUserRaids += userList[i].RaidsCount;
            }
            return totalUserRaids;
        }
    }
}
