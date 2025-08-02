using DiscordBot.Models.Resources;

namespace DiscordBot.Models
{
    public class RaidData
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<SerializedDiscordUser> RaidUsers { get; set; } = [];
        public TitaniumOre TitaniumOre { get; set; } = new(0);
        public StravidiumOre StravidiumOre { get; set; } = new(0);
        public StravidiumFiber StravidiumFiber { get; set; } = new(0);
        public PlastaniumIngot PlastaniumIngot { get; set; } = new(0);
        public Melange Melange { get; set; } = new(0);
        public Sand Sand { get; set; } = new(0);

        public Dictionary<string, StoredUserBets> BetResources { get; set; } = [];

        public RaidData Clone()
        {
            RaidData raidData = new()
            {
                CreatedAt = CreatedAt,
                RaidUsers = RaidUsers,
                TitaniumOre = new(TitaniumOre.Amount),
                StravidiumOre = new(StravidiumOre.Amount),
                StravidiumFiber = new(StravidiumFiber.Amount),
                PlastaniumIngot = new(PlastaniumIngot.Amount),
                Melange = new(Melange.Amount),
                Sand = new(Sand.Amount),
                BetResources = BetResources
            };
            return raidData;
        }

        public bool ContainsRaidUsername(string username, out SerializedDiscordUser? user)
        {
            for (int i = 0; i < RaidUsers.Count; i++)
            {
                if (RaidUsers[i].Name == username)
                {
                    user = RaidUsers[i];
                    return true;
                }
            }

            user = null;
            return false;
        }
    }
}
