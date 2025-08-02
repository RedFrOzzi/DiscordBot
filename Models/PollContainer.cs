using DiscordBot.Models.Resources;

namespace DiscordBot.Models
{
    public class PollContainer
    {
        public Poll? LastCreatedPoll { get; private set; }
        public RaidData? RaidDataSnapshot { get; private set; }
        public List<UserBet> CurrentUserBets { get; private set; } = [];
        
        public int Answer_1_Count { get; private set; }
        public int Answer_1_TotalResources { get; private set; }
        public int Answer_2_Count { get; private set; }
        public int Answer_2_TotalResources { get; private set; }
        public int Answer_3_Count { get; private set; }
        public int Answer_3_TotalResources { get; private set; }
        public int Answer_4_Count { get; private set; }
        public int Answer_4_TotalResources { get; private set; }

        public bool IsPollExist => LastCreatedPoll != null;

        public void CreateNewPoll(ResourcesEnum betResource, RaidData currentRaidData)
        {
            LastCreatedPoll = new()
            {
                BetResourcesType = betResource,
            };

            RaidDataSnapshot = currentRaidData.Clone();
            CurrentUserBets.Clear();
            Answer_1_Count = 0;
            Answer_2_Count = 0;
            Answer_3_Count = 0;
            Answer_4_Count = 0;
            Answer_1_TotalResources = 0;
            Answer_2_TotalResources = 0;
            Answer_3_TotalResources = 0;
            Answer_4_TotalResources = 0;
        }

        public void DeletePoll()
        {
            LastCreatedPoll = null;
            RaidDataSnapshot = null;
            CurrentUserBets.Clear();
            Answer_1_Count = 0;
            Answer_2_Count = 0;
            Answer_3_Count = 0;
            Answer_4_Count = 0;
            Answer_1_TotalResources = 0;
            Answer_2_TotalResources = 0;
            Answer_3_TotalResources = 0;
            Answer_4_TotalResources = 0;
        }

        public void AddUserBet(SerializedDiscordUser user, int betAmount, int answerNum)
        {
            CurrentUserBets.Add(new(user, betAmount, answerNum));

            switch (answerNum)
            {
                case 1:
                    Answer_1_Count++;
                    Answer_1_TotalResources += betAmount;
                    break;
                case 2:
                    Answer_2_Count++;
                    Answer_2_TotalResources += betAmount;
                    break;
                case 3:
                    Answer_3_Count++;
                    Answer_3_TotalResources += betAmount;
                    break;
                case 4:
                    Answer_4_Count++;
                    Answer_4_TotalResources += betAmount;
                    break;
            }
        }

        public void TryAddPollMessageId(ulong id)
        {
            if (LastCreatedPoll != null && LastCreatedPoll.AnswerButtonsMessageId == null)
            {
                LastCreatedPoll.AnswerButtonsMessageId = id;
            }
        }

        public bool IsUserAlreadyBet(string username)
        {
            foreach (var userBet in CurrentUserBets)
            {
                if (userBet.User?.Name == username)
                {
                    return true;
                }
            }

            return false;
        }

        public int TotalUserRaidsCount()
        {
            if (RaidDataSnapshot == null)
            {
                return 0;
            }

            int totalUserRaids = 0;
            for (int i = 0; i < RaidDataSnapshot.RaidUsers.Count; i++)
            {
                totalUserRaids += RaidDataSnapshot.RaidUsers[i].RaidsCount;
            }
            return totalUserRaids;
        }

        public float GetUserRaidsCountFraction(string username, float totalUserRaids, out int maxUserBet)
        {
            if (RaidDataSnapshot == null || LastCreatedPoll == null || totalUserRaids < 1f)
            {
                maxUserBet = 0;
                return 0;
            }

            for (int i = 0; i < RaidDataSnapshot.RaidUsers.Count; i++)
            {
                if (RaidDataSnapshot.RaidUsers[i].Name != username) { continue; }

                int prevBet = 0;
                if (RaidDataSnapshot.BetResources.TryGetValue(username, out var storedUserBets))
                {
                    storedUserBets.BetResources.TryGetValue(LastCreatedPoll.BetResourcesType, out prevBet);
                }

                float userResAmount = GetResAmount(LastCreatedPoll.BetResourcesType, RaidDataSnapshot);
                float fraction = (float)RaidDataSnapshot.RaidUsers[i].RaidsCount / totalUserRaids;
                maxUserBet = (int)(userResAmount * fraction - prevBet);
                return fraction;
            }

            maxUserBet = 0;
            return 0;
        }


        private static float GetResAmount(ResourcesEnum type, RaidData data)
        {
            return type switch
            {
                ResourcesEnum.Melange => data.Melange.Amount,
                ResourcesEnum.StravidiumOre => data.StravidiumOre.Amount,
                ResourcesEnum.StravidiumFiber => data.StravidiumFiber.Amount,
                ResourcesEnum.TitaniumOre => data.TitaniumOre.Amount,
                ResourcesEnum.PlastaniumIngot => data.PlastaniumIngot.Amount,
                ResourcesEnum.Sand => data.Sand.Amount,
                _ => 0
            };
        }
    }
}
