using DiscordBot.Models;
using DiscordBot.Models.Resources;
using DiscordBot.Utilities;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DiscordBot.CommandModules
{
    [SlashCommand("ресурс", "Изменить количество ресурсов")]
    public class CommandResoursesModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("добавить", "добавляет к общему количеству ресурсов.")]
        public async Task AddResources(
            [SlashCommandParameter(Name = "ресурс", Description = "название ресурса")] ResourcesEnum resources,
            [SlashCommandParameter(Name = "количество", Description = "количество ресурсов")] long amount)
        {
            if (!await this.IsAuthorized()) { return; }
            await UpdateResource(this, resources, amount);
        }

        private static async Task UpdateResource(
            ApplicationCommandModule<ApplicationCommandContext> module,
            ResourcesEnum resourceType,
            long amount)
        {
            if (!RaidFilesLoader.TryLoadRaidData(out var data))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Файл рейда не создан, сперва используй /создать",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await module.RespondAsync(errorMsg);
                return;
            }

            switch (resourceType)
            {
                case ResourcesEnum.Melange:
                    data!.Melange.Amount += amount;
                    break;
                case ResourcesEnum.TitaniumOre:
                    data!.TitaniumOre.Amount += amount;
                    break;
                case ResourcesEnum.StravidiumOre:
                    data!.StravidiumOre.Amount += amount;
                    break;
                case ResourcesEnum.StravidiumFiber:
                    data!.StravidiumFiber.Amount += amount;
                    break;
                case ResourcesEnum.PlastaniumIngot:
                    data!.PlastaniumIngot.Amount += amount;
                    break;
                case ResourcesEnum.Sand:
                    data!.Sand.Amount += amount;
                    break;
                default:
                    break;
            }

            if (!RaidFilesLoader.TrySaveRaidData(data!))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Ресурс не обновлен, файл не записан",
                    Flags = MessageFlags.Ephemeral
                };
                var errorMsg = InteractionCallback.Message(errorMsgProps);

                await module.RespondAsync(errorMsg);
                return;
            }

            InteractionMessageProperties successProps = new()
            {
                Content = $"Ресурс <{GetResourceName(resourceType)}> обновлен, новое значение: {GetResourceAmount(data!, resourceType)}",
                Flags = MessageFlags.Ephemeral
            };
            var successMsg = InteractionCallback.Message(successProps);

            await module.RespondAsync(successMsg);
            return;
        }

        private static string GetResourceName(ResourcesEnum resEnum)
        {
            return resEnum switch
            {
                ResourcesEnum.Melange => "Меланж",
                ResourcesEnum.Sand => "Песок",
                ResourcesEnum.TitaniumOre => "Руда Титана",
                ResourcesEnum.PlastaniumIngot => "Пластановый Слиток",
                ResourcesEnum.StravidiumFiber => "Волокно Стравидия",
                ResourcesEnum.StravidiumOre => "Руда Стравидия",
                _ => ""
            };
        }

        private static long GetResourceAmount(RaidData data, ResourcesEnum resEnum)
        {
            return resEnum switch
            {
                ResourcesEnum.Melange => data.Melange.Amount,
                ResourcesEnum.Sand => data.Sand.Amount,
                ResourcesEnum.TitaniumOre => data.TitaniumOre.Amount,
                ResourcesEnum.PlastaniumIngot => data.PlastaniumIngot.Amount,
                ResourcesEnum.StravidiumFiber => data.StravidiumFiber.Amount,
                ResourcesEnum.StravidiumOre => data.StravidiumOre.Amount,
                _ => 0
            };
        }
    }
}
