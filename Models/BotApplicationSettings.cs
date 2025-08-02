namespace DiscordBot.Models
{
    public class BotApplicationSettings
    {
        public static BotApplicationSettings Instance { get; set; } = null!;

        public Dictionary<ulong, string> ModerationRoles { get; set; } = [];
        public List<ulong> AdminIds { get; set; } = [];

        public const string RaidFileName = "raiders.json";
        public const string SettingsFileName = "settings.json";
        public const string SecretsFileName = "secrets.json";

        public BotApplicationSettings()
        {
            Instance ??= this;
        }

        public bool HasRole(NetCord.Role role)
        {
            foreach (var mRole in ModerationRoles)
            {
                if (mRole.Key == role.Id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAdmin(ulong id)
        {
            for (int i = 0; i < AdminIds.Count; i++)
            {
                if (AdminIds[i] == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
