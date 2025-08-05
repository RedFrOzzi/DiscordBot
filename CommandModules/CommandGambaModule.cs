using DiscordBot.Models;
using DiscordBot.Models.Resources;
using DiscordBot.Utilities;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiscordBot.CommandModules
{
    [SlashCommand("игра", "Игра со ставками из ресурсов")]
    public class CommandGambaModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        readonly RestClient _client;
        readonly PollContainer _container;

        public CommandGambaModule(RestClient client, PollContainer container)
        {
            _client = client;
            _container = container;
        }

        [SubSlashCommand("создать", "Создать опрос для игры")]
        public async Task CreatePollModal(
            [SlashCommandParameter(Name = "ресурс", Description = "Ресурс ставки")] ResourcesEnum type)
        {
            if (!await this.IsAuthorized()) { return; }

            if (!RaidFilesLoader.TryLoadRaidData(out var data))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Ошибка загрузки данных рейда.",
                    Flags = MessageFlags.Ephemeral
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }

            if (_container.IsPollExist && _container.LastCreatedPoll!.AnswerButtonsMessageId != null)
            {
                await _client.DeleteMessageAsync(Context.Channel.Id, (ulong)_container.LastCreatedPoll.AnswerButtonsMessageId);
            }

            _container.CreateNewPoll(type, data!);

            TextInputProperties question = new(Poll.QuestionId, TextInputStyle.Paragraph, "Вопрос")
            {
                Placeholder = "Вопрос",
                Required = true,
            };
            TextInputProperties answer_1 = new(Poll.AnswerOneId, TextInputStyle.Short, "Ответ №1")
            {
                Placeholder = "Ответ",
                Required = true,
            };
            TextInputProperties answer_2 = new(Poll.AnswerTwoId, TextInputStyle.Short, "Ответ №2")
            {
                Placeholder = "Ответ",
                Required = true,
            };
            TextInputProperties answer_3 = new(Poll.AnswerThreeId, TextInputStyle.Short, "Ответ №3 (не обязательный)")
            {
                Placeholder = "Ответ",
                Required = false,
            };
            TextInputProperties answer_4 = new(Poll.AnswerFourId, TextInputStyle.Short, "Ответ №4 (не обязательный)")
            {
                Placeholder = "Ответ",
                Required = false,
            };

            ModalProperties mProps = new(Poll.PollModalId, "Создание игры")
            {
                Components = [question, answer_1, answer_2, answer_3, answer_4]
            };

            await RespondAsync(InteractionCallback.Modal(mProps));
        }

        [SubSlashCommand("завершить", "Выбрать номер верного ответа и закрыть ставки")]
        public async Task CloseGamba(
            [SlashCommandParameter(Name = "ответ", Description = "Номер ответа")] AnswerNumber correctAnswerNum)
        {
            if (!await this.IsAuthorized()) { return; }

            if (!RaidFilesLoader.TryLoadRaidData(out var data) || _container.LastCreatedPoll == null)
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Ошибка загрузки данных рейда.",
                    Flags = MessageFlags.Ephemeral
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }

            if (_container.CurrentUserBets.Count <= 1)
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Игра завершена. Не достаточно игроков для перераспределения ресурсов.",
                    Flags = MessageFlags.Ephemeral
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }

            GetBetsStats(_container.CurrentUserBets, (int)correctAnswerNum, out int totalLosersBets, out int totalWinnersBets);

            for (int i = 0; i < _container.CurrentUserBets.Count; i++)
            {
                var serializedUser = _container.CurrentUserBets[i].User;
                int resourceChange = GetUserResourceChange(_container.CurrentUserBets[i], (int)correctAnswerNum, totalLosersBets, totalWinnersBets);

                if (serializedUser == null) { continue; }

                MakeChangesToStoredWinnings(data!, serializedUser, _container, resourceChange);
            }

            if (!RaidFilesLoader.TrySaveRaidData(data!))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Ошибка записи данных рейда.",
                    Flags = MessageFlags.Ephemeral
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }


            if (_container.IsPollExist && _container.LastCreatedPoll.AnswerButtonsMessageId != null)
            {
                await _client.DeleteMessageAsync(Context.Channel.Id, (ulong)_container.LastCreatedPoll.AnswerButtonsMessageId);
            }

            _container.DeletePoll();

            InteractionMessageProperties seccessMsgProps = new()
            {
                Content = "Ставки приняты и распределены.",
                Flags = MessageFlags.Ephemeral
            };
            await RespondAsync(InteractionCallback.Message(seccessMsgProps));
            return;
        }


        
        private static void GetBetsStats(List<UserBet> bets, int correctAnswer, out int totalLosersBets, out int totalWinnersBets)
        {
            totalLosersBets = 0;
            totalWinnersBets = 0;
            for (int i = 0; i < bets.Count; i++)
            {
                var bet = bets[i];
                if (bet == null) continue;

                if (bet.AnswerNumber == correctAnswer)
                {
                    totalWinnersBets += bet.BetAmount;
                }
                else
                {
                    totalLosersBets += bet.BetAmount;
                }
            }
        }

        private static int GetUserResourceChange(UserBet userBet, int correctAnswerNum, int totalLosersBets, int totalWinnersBets)
        {
            if (correctAnswerNum != userBet.AnswerNumber)
            {
                return userBet.BetAmount * -1;
            }

            float winnerFraction = ((float)userBet.BetAmount) / ((float)totalWinnersBets);
            return (int)(userBet.BetAmount + (float)totalLosersBets * winnerFraction);
        }

        private static void MakeChangesToStoredWinnings(RaidData data, SerializedDiscordUser serializedUser, PollContainer container, int resourceChange)
        {
            //If user already bet previoisly
            if (data.BetResources.TryGetValue(serializedUser.Name, out var storedUserBets))
            {
                //if user bet with current bet resource
                if (storedUserBets.BetResources.TryGetValue(container.LastCreatedPoll!.BetResourcesType, out var userPrevBet))
                {
                    //add to existing
                    storedUserBets.BetResources[container.LastCreatedPoll.BetResourcesType] = userPrevBet + resourceChange;
                }
                else
                {
                    //add item with bet
                    storedUserBets.BetResources.Add(container.LastCreatedPoll.BetResourcesType, resourceChange);
                }
            }
            else
            {
                //create new bet to store
                data!.BetResources.Add(serializedUser.Name, new() { BetResources = { { container.LastCreatedPoll!.BetResourcesType, resourceChange } } });
            }
        }
    }
}
