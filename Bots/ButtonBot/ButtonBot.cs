using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BotFrameworkPoC.Bots
{
    public class ButtonBot: ActivityHandler
    {
        /// <summary>
        /// Creates and sends an activity with suggested actions to the user.
        ///
        /// When the user  clicks one of the buttons the text value from the "CardAction" will be
        /// displayed in the channel just as if the user entered the text. There are multiple
        /// "ActionTypes" that may be used for different situations.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task SendSuggestedActionsAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("What is your favorite color?");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>
                {
                    new()
                        {
                            Title = "Red",
                            Type = ActionTypes.ImBack,
                            Value = "Red",
                            Image = "https://via.placeholder.com/20/FF0000?text=R",
                            ImageAltText = "R"
                        },
                    new()
                        {
                            Title = "Yellow",
                            Type = ActionTypes.ImBack,
                            Value = "Yellow",
                            Image = "https://via.placeholder.com/20/FFFF00?text=Y",
                            ImageAltText = "Y"
                        },
                    new()
                        {
                            Title = "Blue",
                            Type = ActionTypes.ImBack,
                            Value = "Blue",
                            Image = "https://via.placeholder.com/20/0000FF?text=B",
                            ImageAltText = "B"
                        }
                },
            };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
        
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            await SendSuggestedActionsAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}