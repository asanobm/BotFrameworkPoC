namespace BotFrameworkPoC.Bots.StateManagementBot.data
{
    /// <summary>
    /// Conversation data class.
    /// </summary>
    public class ConversationData
    {
        // incoming message timestamp.
        public string Timestamp { get; set; }
        // user's channel id
        public string ChannelId { get; set; }
        // Track whether we have already asked the user's name
        public bool PromptedUserForName { get; set; }
    }
}