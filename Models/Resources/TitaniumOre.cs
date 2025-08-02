namespace DiscordBot.Models.Resources
{
    public class TitaniumOre : IResource
    {
        public long Amount { get; set; } = 0;

        public TitaniumOre(long amount)
        {
            Amount = amount;
        }
    }
}
