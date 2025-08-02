namespace DiscordBot.Models.Resources
{
    public class StravidiumFiber : IResource
    {
        public long Amount { get; set; } = 0;
        public StravidiumFiber(long amount)
        {
            Amount = amount;
        }
    }
}
