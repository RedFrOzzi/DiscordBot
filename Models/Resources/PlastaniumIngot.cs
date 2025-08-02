namespace DiscordBot.Models.Resources
{
    public class PlastaniumIngot : IResource
    {
        public long Amount { get; set; } = 0;
        public PlastaniumIngot(long amount)
        {
            Amount = amount;
        }
    }
}
