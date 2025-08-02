using DiscordBot.Models;
using DiscordBot.Models.Resources;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace DiscordBot.ComponentInteractionModules
{
    public class ButtonInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
    {
        readonly PollContainer _container;

        public ButtonInteractionModule(PollContainer container)
        {
            _container = container;
        }

        [ComponentInteraction(Poll.ButtonAnswer_1_Id)]
        public async Task ButtonAnswer_1()
        {
            await SendAnswerModal(1);
        }

        [ComponentInteraction(Poll.ButtonAnswer_2_Id)]
        public async Task ButtonAnswer_2()
        {
            await SendAnswerModal(2);
        }

        [ComponentInteraction(Poll.ButtonAnswer_3_Id)]
        public async Task ButtonAnswer_3()
        {
            await SendAnswerModal(3);
        }

        [ComponentInteraction(Poll.ButtonAnswer_4_Id)]
        public async Task ButtonAnswer_4()
        {
            await SendAnswerModal(4);
        }

        private async Task SendAnswerModal(int answerNum)
        {
            if (!_container.IsPollExist)
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Вопрос был утерян, без возможности восстановления. Придется создавать новый :("
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }

            if (_container.IsUserAlreadyBet(Context.User.Username))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = $"{Context.User.Username} уже делал ставку!"
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }

            var resType = _container.LastCreatedPoll!.BetResourcesType;
            var totalRaidsCount = _container.TotalUserRaidsCount();
            _container.GetUserRaidsCountFraction(Context.User.Username, totalRaidsCount, out var maxUserBet);

            TextInputProperties bet = new(Poll.BetReosourceId, TextInputStyle.Short, $"Ставка: {IResource.GetResName(resType)}")
            {
                Placeholder = $"Укажи количество вплоть до: {maxUserBet}",
                Required = true,
            };

            ModalProperties mProps = new($"{Poll.BetModalId}:{answerNum}:{maxUserBet}", $"Ставка ответ № {answerNum}")
            {
                Components = [bet]
            };

            var calback = InteractionCallback.Modal(mProps);
            await RespondAsync(calback);
        }
    }
}
