using DiscordBot.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DiscordBot.Utilities
{
    internal class RaidFilesLoader
    {
        static readonly JsonSerializerOptions _jsonSerializerOptions = new() 
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true,
            };

        public static bool TrySaveSettings(BotApplicationSettings settings)
        {
            try
            {
                var dirPath = AppDomain.CurrentDomain.BaseDirectory;
                var fullPath = dirPath + BotApplicationSettings.SettingsFileName;
                var jsonSettings = JsonSerializer.Serialize(settings, _jsonSerializerOptions);
                File.WriteAllText(fullPath, jsonSettings);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Dictionary<ulong, string> LoadSettings()
        {
            var dirPath = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = dirPath + BotApplicationSettings.SettingsFileName;
            if (!File.Exists(fullPath))
            {
                return [];
            }

            try
            {
                var jsonFile = File.ReadAllText(fullPath);
                var settings = JsonSerializer.Deserialize<BotApplicationSettings>(jsonFile);
                return settings == null ? [] : settings.ModerationRoles;
            }
            catch
            {
                return [];
            }
        }

        public static bool TrySaveRaidData(RaidData data)
        {
            try
            {
                var dirPath = AppDomain.CurrentDomain.BaseDirectory;
                var fullPath = dirPath + BotApplicationSettings.RaidFileName;
                var jsonRaidData = JsonSerializer.Serialize(data, _jsonSerializerOptions);
                File.WriteAllText(fullPath, jsonRaidData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryLoadRaidData(out RaidData? raidData)
        {
            var dirPath = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = dirPath + BotApplicationSettings.RaidFileName;
            if (!File.Exists(fullPath))
            {
                raidData = null;
                return false;
            }

            try
            {
                var jsonFile = File.ReadAllText(fullPath);
                var data = JsonSerializer.Deserialize<RaidData>(jsonFile);
                raidData = data;
                return true;
            }
            catch
            {
                raidData = null;
                return false;
            }
        }
    }
}
