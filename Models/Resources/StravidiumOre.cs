namespace DiscordBot.Models.Resources
{
    public class StravidiumOre : IResource
    {
        public long Amount { get; set; } = 0;
        public StravidiumOre(long amount)
        {
            Amount = amount;
        }
    }
}
