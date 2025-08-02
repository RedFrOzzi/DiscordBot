using NetCord.Services.ApplicationCommands;

namespace DiscordBot.Models.Resources
{
    public enum ResourcesEnum
    {
        [SlashCommandChoice(Name = "меланж")]
        Melange = 0,
        [SlashCommandChoice(Name = "пластановый слиток")]
        PlastaniumIngot = 1,
        [SlashCommandChoice(Name = "песок")]
        Sand = 2,
        [SlashCommandChoice(Name = "руда стравидия")]
        StravidiumOre = 3,
        [SlashCommandChoice(Name = "волокно стравидия")]
        StravidiumFiber = 4,
        [SlashCommandChoice(Name = "руда титана")]
        TitaniumOre = 5,
    }
}
