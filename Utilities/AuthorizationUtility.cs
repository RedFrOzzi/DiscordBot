using DiscordBot.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DiscordBot.Utilities
{
    internal static class AuthorizationUtility
    {
        public static async Task<bool> IsAuthorized(this ApplicationCommandModule<ApplicationCommandContext> module)
        {
            var guildUser = module.Context.User as GuildUser;

            if (guildUser == null || module.Context.Guild == null)
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Ошибка пользователя"
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await module.RespondAsync(errorMsg);
                return false;
            }

            var roles = guildUser.GetRoles(module.Context.Guild);
            bool isAllowed = false;
            foreach (var role in roles)
            {
                if (BotApplicationSettings.Instance.HasRole(role))
                {
                    isAllowed = true;
                    break;
                }
            }

            if (!isAllowed && !BotApplicationSettings.Instance.IsAdmin(module.Context.User.Id))
            {
                InteractionMessageProperties imsgp = new()
                {
                    Content = "Роль пользователя не авторизована"
                };
                var errorMsg = InteractionCallback.Message(imsgp);

                await module.RespondAsync(errorMsg);
                return false;
            }

            return true;
        }
    }
}
