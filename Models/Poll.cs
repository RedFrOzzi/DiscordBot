using DiscordBot.Models.Resources;

namespace DiscordBot.Models
{
    public class Poll
    {
        public ulong? AnswerButtonsMessageId { get; set; }
        public ResourcesEnum BetResourcesType { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer_1 { get; set; } = string.Empty;
        public string Answer_2 { get; set; } = string.Empty;
        public string Answer_3 { get; set; } = string.Empty;
        public string Answer_4 { get; set; } = string.Empty;

        public Dictionary<string, int> Answer_1_Voters { get; set; } = [];
        public Dictionary<string, int> Answer_2_Voters { get; set; } = [];
        public Dictionary<string, int> Answer_3_Voters { get; set; } = [];
        public Dictionary<string, int> Answer_4_Voters { get; set; } = [];


        //---------MODAL-CONSTANTS----------------------
        public const string PollModalId = "pollmodal";
        public const string QuestionId = "question";
        public const string AnswerOneId = "answerone";
        public const string AnswerTwoId = "answertwo";
        public const string AnswerThreeId = "answerthree";
        public const string AnswerFourId = "answerfour";
        //---------BUTTON-CONSTANTS---------------------
        public const string ButtonAnswer_1_Id = "buttonanswerone";
        public const string ButtonAnswer_2_Id = "buttonanswertwo";
        public const string ButtonAnswer_3_Id = "buttonanswerthree";
        public const string ButtonAnswer_4_Id = "buttonanswerfour";

        public const string BetModalId = "betmodal";
        public const string BetReosourceId = "betresource";
    }
}
