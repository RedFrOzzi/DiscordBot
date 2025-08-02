namespace DiscordBot.Models.Resources
{
    internal interface IResource
    {
        public long Amount { get; set; }

        public static string GetResName(ResourcesEnum resType)
        {
            return resType switch
            {
                ResourcesEnum.Melange => "Меланж",
                ResourcesEnum.PlastaniumIngot => "Пластановый слиток",
                ResourcesEnum.Sand => "Песок",
                ResourcesEnum.StravidiumFiber => "Волокно Стравидия",
                ResourcesEnum.StravidiumOre => "Руда Стравидия",
                ResourcesEnum.TitaniumOre => "Руда Титана",
                _ => "хз"
            };
        }
    }
}
