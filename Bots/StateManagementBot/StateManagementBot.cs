using System;
using System.Threading;
using System.Threading.Tasks;
using BotFrameworkPoC.Bots.StateManagementBot.data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;


namespace BotFrameworkPoC.Bots.StateManagementBot
{
    public class StateManagementBot: ActivityHandler
    {
        private BotState _userState;
        private BotState _conversationState;

        public StateManagementBot(
            UserState userState,
            ConversationState conversationState)
        {
            _userState = userState;
            _conversationState = conversationState;
        }

        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Create Conversation state accessors.
            var conversationStateAccessors =
                _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());
            
            
            // Create User state accessors.
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // First time around this is set to false,
                // so we will prompt user for name.
                if (conversationData.PromptedUserForName)
                {
                    // Set the name to what the user provided.
                    userProfile.Name = turnContext.Activity.Text?.Trim();
                    
                    // Acknowledge that we got their name.
                    await turnContext.SendActivityAsync(
                        $"Thanks {userProfile.Name}. To see conversation data, type anything.",
                        cancellationToken: cancellationToken);
                    
                    // Reset the flag to allow the bot to go through the cycle again.
                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    // Prompt the user for their name.
                    await turnContext.SendActivityAsync(
                        "What is your name?",
                        cancellationToken: cancellationToken);
                    
                    // Set the flag to true, so we don't prompt in the next turn.
                    conversationData.PromptedUserForName = true;
                }
            }
            else
            {
                // Add message details to the conversation data.
                // Convert saved Timestamp to local DataTimeOffset, then to string for display.
                if (turnContext.Activity.Timestamp != null)
                {
                    var messageTimeOffset = (DateTimeOffset) turnContext.Activity.Timestamp;
                    var localMessageTime = messageTimeOffset.ToLocalTime();
                    conversationData.Timestamp = localMessageTime.ToString();
                }

                conversationData.ChannelId = turnContext.Activity.ChannelId;
                
                // Display state data.
                await turnContext.SendActivityAsync(
                    $"{userProfile.Name} sent: {turnContext.Activity.Text}",
                    cancellationToken: cancellationToken);
                
                await turnContext.SendActivityAsync(
                    $"Message received at: {conversationData.Timestamp}",
                    cancellationToken: cancellationToken);
                
                await turnContext.SendActivityAsync(
                    $"Message received from: {conversationData.ChannelId} ",
                    cancellationToken: cancellationToken);
            }
        }

        public override async Task OnTurnAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = new())
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}