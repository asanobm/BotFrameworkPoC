using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BotFrameworkPoC.Bots
{
    /// <summary>
    /// Send a Typing indicator
    /// Users expect a timely response to their messages.
    /// </summary>
    public class SendTypingIndicatorBot: ActivityHandler
    {
        /// <summary>
        /// When the user first calls the bot.
        /// </summary>
        /// <param name="membersAdded">Channel Account list.</param>
        /// <param name="turnContext">turn context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Hello Message</returns>
        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // return async activity with a text message.
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text(
                            welcomeText,
                            welcomeText),
                        cancellationToken);
                }
            }
        }

        /// <summary>
        /// Users expect a timely response to their messages.
        /// </summary>
        /// <param name="turnContext"> turn context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>
        /// async response
        /// 1. Typing activity
        /// 2. wait 3000 m/s
        /// 3. text message.
        /// </returns>
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (string.Equals(
                turnContext.Activity.Text,
                "wait",
                System.StringComparison.InvariantCultureIgnoreCase))
            {
                await turnContext.SendActivitiesAsync(
                    new IActivity[]
                    {
                        new Activity { Type = ActivityTypes.Typing },
                        new Activity { Type = "delay", Value = 3000 },
                        MessageFactory.Text("Finished typing", "Finished typing")
                    },
                    cancellationToken);
            }
            else
            {
                var replyText = $"Echo: {turnContext.Activity.Text}. Say 'wait' to watch me type.";
                
                // return async activity
                await turnContext.SendActivityAsync(
                    MessageFactory.Text(
                        replyText,
                        replyText),
                    cancellationToken);
            }
        }
    }
}