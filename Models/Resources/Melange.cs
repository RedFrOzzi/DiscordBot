namespace DiscordBot.Models.Resources
{
    public class Melange : IResource
    {
        public long Amount { get; set; } = 0;
        public Melange(long amount)
        {
            Amount = amount;
        }
    }
}
