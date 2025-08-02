using DiscordBot.Models.Resources;

namespace DiscordBot.Models
{
    public class StoredUserBets
    {
        public Dictionary<ResourcesEnum, int> BetResources { get; set; } = [];
    }
}
