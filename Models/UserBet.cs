namespace DiscordBot.Models
{
    public class UserBet
    {
        public UserBet(SerializedDiscordUser user, int betAmount, int answerNumber)
        {
            User = user;
            BetAmount = betAmount;
            AnswerNumber = answerNumber;
        }

        public SerializedDiscordUser? User { get; private set; }
        public int BetAmount { get; private set; }
        public int AnswerNumber { get; private set; }
    }
}
