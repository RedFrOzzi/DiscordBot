namespace DiscordBot.Models.Resources
{
    public class Sand : IResource
    {
        public long Amount { get; set; } = 0;
        public Sand(long amount)
        {
            Amount = amount;
        }
    }
}
