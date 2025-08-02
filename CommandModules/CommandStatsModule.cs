using DiscordBot.Models;
using DiscordBot.Models.Resources;
using DiscordBot.Utilities;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Text;

namespace DiscordBot.CommandModules
{
    public class CommandStatsModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        readonly StringBuilder _stringBuilder;

        public CommandStatsModule()
        {
            _stringBuilder = new();
        }

        [SlashCommand("показать_распределение", "Показывает текущие данные рейда")]
        public async Task ShowStats()
        {
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

            var embedProperties = new EmbedFieldProperties[data!.RaidUsers.Count];
            int totalUserRaidsCount = TotalUserRaidsCount(data.RaidUsers);

            for (int i = 0; i < data!.RaidUsers.Count; i++)
            {
                var fractionOfUserRaidCount = GetUserRaidsCountFraction(data.RaidUsers[i], totalUserRaidsCount);
                var userStats = BuildStatString(_stringBuilder, data.RaidUsers[i], data, fractionOfUserRaidCount);

                if (string.IsNullOrEmpty(data.RaidUsers[i].NickName))
                {
                    if (string.IsNullOrEmpty(data.RaidUsers[i].GlobaName))
                    {
                        embedProperties[i] = new()
                        {
                            Name = $"{data!.RaidUsers[i].Name}",
                            Value = userStats
                        };
                        continue;
                    }
                    else
                    {
                        embedProperties[i] = new()
                        {
                            Name = $"{data!.RaidUsers[i].Name} ({data.RaidUsers[i].GlobaName})",
                            Value = userStats
                        };
                        continue;
                    }
                }

                embedProperties[i] = new()
                {
                    Name = $"{data!.RaidUsers[i].Name} ({data.RaidUsers[i].NickName})",
                    Value = userStats
                };
            }

            EmbedProperties embedOverall = new()
            {
                Title = "Общие собранные ресурсы",
                Description = BuildOverallStats(_stringBuilder, data),
                Color = new(0x97099e)
            };

            EmbedProperties embedStats = new()
            {
                Title = "Рейд",
                Description = "Текущее распределение:",
                Fields = embedProperties,
                Color = new(0x065703)
            };

            InteractionMessageProperties interactionMessageProperties = new()
            {
                Embeds = [embedOverall, embedStats]
            };
            var message = InteractionCallback.Message(interactionMessageProperties);
            await RespondAsync(message);
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

        private static float GetUserRaidsCountFraction(SerializedDiscordUser user, float totalUserRaids)
        {
            if (totalUserRaids < 1f) { return 0; }

            return (float)user.RaidsCount / totalUserRaids;
        }

        private static string BuildStatString(StringBuilder builder, SerializedDiscordUser user, RaidData data, float fractionOfResources)
        {
            builder.Clear();
            builder.AppendLine($"Количество рейдов: {user.RaidsCount}");

            int melangeBetAmount = BetResource(ResourcesEnum.Melange, user, data);
            int melangeValue = (int)((float)data.Melange.Amount * fractionOfResources);
            if (melangeBetAmount == 0)
            {
                builder.AppendLine($"Меланж: {melangeValue}");
            }
            else
            {
                builder.AppendLine($"Меланж: Общее: {melangeValue}, Выиграл: {melangeBetAmount}, К выдаче: {melangeValue + melangeBetAmount}");
            }

            int plastaniumBetAmount = BetResource(ResourcesEnum.PlastaniumIngot, user, data);
            int plastaniumValue = (int)((float)data.PlastaniumIngot.Amount * fractionOfResources);
            if (plastaniumBetAmount == 0)
            {
                builder.AppendLine($"Пластановый слиток: {plastaniumValue}");
            }
            else
            {
                builder.AppendLine($"Пластановый слиток: Общее: {plastaniumValue}, Выиграл: {plastaniumBetAmount}, К выдаче: {plastaniumValue + plastaniumBetAmount}");
            }

            int sandBetAmount = BetResource(ResourcesEnum.Sand, user, data);
            int sandValue = (int)((float)data.Sand.Amount * fractionOfResources);
            if (sandBetAmount == 0)
            {
                builder.AppendLine($"Песок: {sandValue}");
            }
            else
            {
                builder.AppendLine($"Песок: Общее: {sandValue}, Выиграл: {sandBetAmount}, К выдаче: {sandValue + sandBetAmount}");
            }

            int stravidiumBetAmount = BetResource(ResourcesEnum.StravidiumOre, user, data);
            int stravidiumOreValue = (int)((float)data.StravidiumOre.Amount * fractionOfResources);
            if (stravidiumBetAmount == 0)
            {
                builder.AppendLine($"Руда Стравидия: {stravidiumOreValue}");
            }
            else
            {
                builder.AppendLine($"Руда Стравидия: Общее: {stravidiumOreValue}, Выиграл: {stravidiumBetAmount}, К выдаче: {stravidiumOreValue + stravidiumBetAmount}");
            }

            int stravidiumFiberBetAmount = BetResource(ResourcesEnum.StravidiumFiber, user, data);
            int stravidiumFiberValue = (int)((float)data.StravidiumFiber.Amount * fractionOfResources);
            if (stravidiumFiberBetAmount == 0)
            {
                builder.AppendLine($"Волокно Стравидия: {stravidiumFiberValue}");
            }
            else
            {
                builder.AppendLine($"Волокно Стравидия: Общее: {stravidiumFiberValue}, Выиграл: {stravidiumFiberBetAmount}, К выдаче: {stravidiumFiberValue + stravidiumFiberBetAmount}");
            }

            int titaniumBetAmount = BetResource(ResourcesEnum.TitaniumOre, user, data);
            int titaniumValue = (int)((float)data.TitaniumOre.Amount * fractionOfResources);
            if (titaniumBetAmount == 0)
            {
                builder.AppendLine($"Титановая руда: {titaniumValue}");
            }
            else
            {
                builder.AppendLine($"Титановая руда: Общее: {titaniumValue}, Выиграл: {titaniumBetAmount}, К выдаче: {titaniumValue + titaniumBetAmount}");
            }

            return builder.ToString();
        }

        private static int BetResource(ResourcesEnum type, SerializedDiscordUser user, RaidData data)
        {
            int resAmount = 0;
            if (data.BetResources.TryGetValue(user.Name, out var storedUserBets))
            {
                storedUserBets.BetResources.TryGetValue(type, out resAmount);
            }
            return resAmount;
        }

        private static string BuildOverallStats(StringBuilder builder, RaidData data)
        {
            builder.Clear();

            builder.AppendLine($"Меланж: {data.Melange.Amount}");
            builder.AppendLine($"Пластановый слиток: {data.PlastaniumIngot.Amount}");
            builder.AppendLine($"Песок: {data.Sand.Amount}");
            builder.AppendLine($"Руда Стравидия: {data.StravidiumOre.Amount}");
            builder.AppendLine($"Волокно Стравидия: {data.StravidiumFiber.Amount}");
            builder.AppendLine($"Титановая руда: {data.TitaniumOre.Amount}");

            return builder.ToString();
        }
    }
}
