using DiscordBot.Models;
using DiscordBot.Models.Resources;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using System.Text;

namespace DiscordBot.ComponentInteractionModules
{
    public class PollModalInteractionModule : ComponentInteractionModule<ModalInteractionContext>
    {
        readonly RestClient _client;
        readonly PollContainer _container;

        public PollModalInteractionModule(RestClient client, PollContainer container)
        {
            _client = client;
            _container = container;
        }

        [ComponentInteraction(Poll.PollModalId)]
        public async Task RespondToPollModalCreation()
        {
            var poll = _container.LastCreatedPoll;
            if (Context == null || Context.Components == null || Context.Components.Count == 0 || poll == null)
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Внутренняя ошибка"
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }

            ButtonProperties buttonAnswer_1 = new(Poll.ButtonAnswer_1_Id, "Ответ № 1", NetCord.ButtonStyle.Primary);
            ButtonProperties buttonAnswer_2 = new(Poll.ButtonAnswer_2_Id, "Ответ № 2", NetCord.ButtonStyle.Primary);
            ButtonProperties? buttonAnswer_3 = null;
            ButtonProperties? buttonAnswer_4 = null;

            EmbedFieldProperties? fieldAnswer_1 = null;
            EmbedFieldProperties? fieldAnswer_2 = null;
            EmbedFieldProperties? fieldAnswer_3 = null;
            EmbedFieldProperties? fieldAnswer_4 = null;

            foreach (var component in Context.Components)
            {
                if (component is not TextInput input) { continue; }

                if (input.CustomId == Poll.QuestionId)
                {
                    poll.Question = input.Value;
                    continue;
                }

                if (input.CustomId == Poll.AnswerOneId)
                {
                    poll.Answer_1 = input.Value;
                    fieldAnswer_1 = new() { Name = "Ответ № 1:", Value = $"{poll.Answer_1}\n< Ставок: 0 | Всего ресурсов: 0 >" };
                    continue;
                }

                if (input.CustomId == Poll.AnswerTwoId)
                {
                    poll.Answer_2 = input.Value;
                    fieldAnswer_2 = new() { Name = "Ответ № 2:", Value = $"{poll.Answer_2}\n< Ставок: 0 | Всего ресурсов: 0 >" };
                    continue;
                }

                if (input.CustomId == Poll.AnswerThreeId)
                {
                    if (string.IsNullOrEmpty(input.Value))
                    {
                        poll.Answer_3 = string.Empty;
                        continue;
                    }

                    poll.Answer_3 = input.Value;
                    buttonAnswer_3 = new(Poll.ButtonAnswer_3_Id, "Ответ № 3", NetCord.ButtonStyle.Primary);
                    fieldAnswer_3 = new() { Name = "Ответ № 3:", Value = $"{poll.Answer_3}\n< Ставок: 0 | Всего ресурсов: 0 >" };
                    continue;
                }

                if (input.CustomId == Poll.AnswerFourId)
                {
                    if (string.IsNullOrEmpty(input.Value))
                    {
                        poll.Answer_4 = string.Empty;
                        continue;
                    }

                    poll.Answer_4 = input.Value;
                    buttonAnswer_4 = new(Poll.ButtonAnswer_4_Id, "Ответ № 4", NetCord.ButtonStyle.Primary);
                    fieldAnswer_4 = new() { Name = "Ответ № 4:", Value = $"{poll.Answer_4}\n< Ставок: 0 | Всего ресурсов: 0 >" };
                    continue;
                }
            }

            InteractionMessageProperties props = new();

            ActionRowProperties actionRow = new()
            {
                Buttons = [buttonAnswer_1, buttonAnswer_2]
            };
            if (buttonAnswer_3 != null)
            {
                actionRow.AddButtons(buttonAnswer_3);
            }
            if (buttonAnswer_4 != null)
            {
                actionRow.AddButtons(buttonAnswer_4);
            }

            EmbedProperties embedProps = new()
            {
                Title = poll.Question,
                Description = $"Ресурс для ставки: {IResource.GetResName(poll.BetResourcesType)}",
                Fields = [fieldAnswer_1!, fieldAnswer_2!],
                Color = new(0xd90f3e)
            };
            if (fieldAnswer_3 != null)
            {
                embedProps.AddFields(fieldAnswer_3);
            }
            if (fieldAnswer_4 != null)
            {
                embedProps.AddFields(fieldAnswer_4);
            }

            props.Components = [actionRow];
            props.Embeds = [embedProps];
            var message = await RespondAsync(InteractionCallback.Message(props), true);
            if (message == null || message.Resource == null || message.Resource.Message == null) { return; }

            _container.TryAddPollMessageId(message.Resource.Message.Id);
        }

        [ComponentInteraction(Poll.BetModalId)]
        public async Task RespondToAnswerGiven(int answerNum, int maxUserBetAmount)
        {
            var poll = _container.LastCreatedPoll;
            var raid = _container.RaidDataSnapshot;
            if (Context == null || Context.Components == null || Context.Components.Count == 0 || poll == null || raid == null || poll.AnswerButtonsMessageId == null)
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = "Внутренняя ошибка"
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }

            if (!raid.ContainsRaidUsername(Context.User.Username, out var guildUser))
            {
                InteractionMessageProperties errorMsgProps = new()
                {
                    Content = $"Пользователь с ником {Context.User.Username}({((GuildUser)Context.User).Nickname}) не имеет ресурсов."
                };
                await RespondAsync(InteractionCallback.Message(errorMsgProps));
                return;
            }

            foreach (var component in Context.Components)
            {
                if (component is not TextInput input || input.CustomId != Poll.BetReosourceId) { continue; }
                
                if (!int.TryParse(input.Value, out int resAmount))
                {
                    GuildUser? user = Context.User as GuildUser;
                    string? name = string.IsNullOrEmpty(user?.Nickname) ? user?.Nickname : user?.Username;
                    name ??= string.Empty;
                    InteractionMessageProperties errorMsgProps2 = new()
                    {
                        Content = $"{name} ввел не правильное значение ставки."
                    };
                    await RespondAsync(InteractionCallback.Message(errorMsgProps2));
                    return;
                }

                if (resAmount <= 0)
                {
                    InteractionMessageProperties errorMsgProps = new()
                    {
                        Content = $"Пользователь с ником {guildUser!.Name}({guildUser.NickName}) пытается поставить 0 ресурсов 🤨"
                    };
                    await RespondAsync(InteractionCallback.Message(errorMsgProps));
                    return;
                }

                if (resAmount > maxUserBetAmount)
                {
                    InteractionMessageProperties errorMsgProps = new()
                    {
                        Content = $"Пользователь с ником {guildUser!.Name}({guildUser.NickName}) пытается поставить больше чем ему полагается 🤨"
                    };
                    await RespondAsync(InteractionCallback.Message(errorMsgProps));
                    return;
                }

                _container.AddUserBet(guildUser!, resAmount, answerNum);

                await _client.ModifyMessageAsync(Context.Channel.Id, (ulong)poll.AnswerButtonsMessageId, RebuildMessage);
                await RespondAsync(InteractionCallback.ModifyMessage(opt => { }));

                void RebuildMessage(MessageOptions options)
                {
                    ButtonProperties buttonAnswer_1 = new(Poll.ButtonAnswer_1_Id, "Ответ № 1", NetCord.ButtonStyle.Primary);
                    ButtonProperties buttonAnswer_2 = new(Poll.ButtonAnswer_2_Id, "Ответ № 2", NetCord.ButtonStyle.Primary);
                    ButtonProperties? buttonAnswer_3 = null;
                    ButtonProperties? buttonAnswer_4 = null;

                    EmbedFieldProperties? fieldAnswer_1 = null;
                    EmbedFieldProperties? fieldAnswer_2 = null;
                    EmbedFieldProperties? fieldAnswer_3 = null;
                    EmbedFieldProperties? fieldAnswer_4 = null;

                    fieldAnswer_1 = new() { Name = "Ответ № 1:", Value = $"{poll.Answer_1}\n< Ставок: {_container.Answer_1_Count} | Всего ресурсов: {_container.Answer_1_TotalResources} >" };
                    fieldAnswer_2 = new() { Name = "Ответ № 2:", Value = $"{poll.Answer_2}\n< Ставок: {_container.Answer_2_Count} | Всего ресурсов: {_container.Answer_2_TotalResources} >" };

                    if (!string.IsNullOrEmpty(poll.Answer_3))
                    {
                        buttonAnswer_3 = new(Poll.ButtonAnswer_3_Id, "Ответ № 3", NetCord.ButtonStyle.Primary);
                        fieldAnswer_3 = new() { Name = "Ответ № 3:", Value = $"{poll.Answer_3}\n< Ставок: {_container.Answer_3_Count} | Всего ресурсов: {_container.Answer_3_TotalResources} >" };
                    }

                    if (!string.IsNullOrEmpty(poll.Answer_4))
                    {
                        buttonAnswer_4 = new(Poll.ButtonAnswer_4_Id, "Ответ № 4", NetCord.ButtonStyle.Primary);
                        fieldAnswer_4 = new() { Name = "Ответ № 4:", Value = $"{poll.Answer_4}\n< Ставок: {_container.Answer_4_Count} | Всего ресурсов: {_container.Answer_4_TotalResources} >" };
                    }

                    ActionRowProperties actionRow = new()
                    {
                        Buttons = [buttonAnswer_1, buttonAnswer_2]
                    };
                    if (buttonAnswer_3 != null)
                    {
                        actionRow.AddButtons(buttonAnswer_3);
                    }
                    if (buttonAnswer_4 != null)
                    {
                        actionRow.AddButtons(buttonAnswer_4);
                    }

                    EmbedProperties embedProps = new()
                    {
                        Title = poll.Question,
                        Fields = [fieldAnswer_1!, fieldAnswer_2!],
                        Color = new(0xd90f3e)
                    };
                    if (fieldAnswer_3 != null)
                    {
                        embedProps.AddFields(fieldAnswer_3);
                    }
                    if (fieldAnswer_4 != null)
                    {
                        embedProps.AddFields(fieldAnswer_4);
                    }

                    options.Components = [actionRow];
                    options.Embeds = [embedProps];
                }
            }
        }
    }
}
