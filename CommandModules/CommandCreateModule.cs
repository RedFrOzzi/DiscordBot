using DiscordBot.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiscordBot.CommandModules
{
    public class CommandCreateModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        readonly JsonSerializerOptions _jsonSerializerOptions;

        public CommandCreateModule()
        {
            _jsonSerializerOptions = new() 
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true,
            };
        }
        
        [SlashCommand("создать_новый_рейд", "Создать файл рейда с участниками и ресурсами")]
        public async Task CreateChannelList(
            [SlashCommandParameter(Name = "канал", Description = "канал в котором находятся участники рейда")] VoiceGuildChannel channel)
        {
            var partialUser = Context.User as GuildUser;

            if (partialUser == null || Context.Guild == null)
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Ошибка пользователя"
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            var roles = partialUser.GetRoles(Context.Guild);
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
                return;
            }

            var voiceUsers = Context?.Guild?.VoiceStates;

            if (voiceUsers == null || voiceUsers.Count == 0)
            {
                RaidData rData = new();
                var jsonFile = JsonSerializer.Serialize(rData, _jsonSerializerOptions);
                var filePath = AppDomain.CurrentDomain.BaseDirectory + BotApplicationSettings.RaidFileName;
                File.WriteAllText(filePath, jsonFile);

                InteractionMessageProperties imsgp = new()
                {
                    Content = "Голосовые каналы не имеют активных пользователей, создан пустой файл рейда."
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await RespondAsync(errorMsg);
                return;
            }

            List<SerializedDiscordUser> toSaveUsers = new(voiceUsers.Count);

            foreach (var user in voiceUsers)
            {
                if (user.Value?.ChannelId == channel.Id)
                {
                    var name = user.Value?.User?.Username;
                    if (name == null)
                    {
                        continue;
                    }
                    var nickname = user.Value?.User?.Nickname;
                    var globalName = user.Value?.User?.GlobalName;
                    SerializedDiscordUser raidUser = new(user.Key, name, 1)
                    {
                        NickName = nickname ?? string.Empty,
                        GlobaName = globalName ?? string.Empty
                    };

                    toSaveUsers.Add(raidUser);
                }
            }

            //If channel users count is zero then abort action
            if (toSaveUsers.Count == 0)
            {
                RaidData rData = new();
                var jsonFile = JsonSerializer.Serialize(rData, _jsonSerializerOptions);
                var filePath = AppDomain.CurrentDomain.BaseDirectory + BotApplicationSettings.RaidFileName;
                File.WriteAllText(filePath, jsonFile);

                InteractionMessageProperties zeroUsersMsgProps = new()
                {
                    Content = "Указанный голосовой канал не имеет активных пользователей, создан пустой файл рейда."
                };
                var zeroUsersMsg = InteractionCallback.Message(zeroUsersMsgProps);

                await RespondAsync(zeroUsersMsg);
                return;
            }

            var embedProperties = new EmbedFieldProperties[toSaveUsers.Count];

            for (int i = 0; i < toSaveUsers.Count; i++)
            {
                if (string.IsNullOrEmpty(toSaveUsers[i].NickName))
                {
                    if (string.IsNullOrEmpty(toSaveUsers[i].GlobaName))
                    {
                        embedProperties[i] = new()
                        {
                            Name = $"{toSaveUsers[i].Name}"
                        };
                        continue;
                    }
                    else
                    {
                        embedProperties[i] = new()
                        {
                            Name = $"{toSaveUsers[i].Name} ({toSaveUsers[i].GlobaName})"
                        };
                        continue;
                    }
                }

                embedProperties[i] = new()
                {
                    Name = $"{toSaveUsers[i].Name} ({toSaveUsers[i].NickName})"
                };
            }

            EmbedProperties embed = new()
            {
                Title = "Рейд",
                Description = "Текущий состав:",
                Fields = embedProperties
            };

            InteractionMessageProperties interactionMessageProperties = new()
            {
                Embeds = [embed]
            };
            var message = InteractionCallback.Message(interactionMessageProperties);

            RaidData raidData = new()
            {
                RaidUsers = toSaveUsers,
            };
            var raiderUsersJson = JsonSerializer.Serialize(raidData, _jsonSerializerOptions);
            var fullPath = AppDomain.CurrentDomain.BaseDirectory + BotApplicationSettings.RaidFileName;
            File.WriteAllText(fullPath, raiderUsersJson);

            await RespondAsync(message);
        }
    }
}
