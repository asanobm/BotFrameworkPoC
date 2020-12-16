using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace BotFrameworkPoC.Bots
{
    public class AttachmentBot: ActivityHandler
    {
        
        /// <summary>
        /// Display user select options
        ///
        /// create "HeroCard" for user actions.
        /// the user could use buttons.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>HeroCard</returns>
        private static async Task DisplayOptionsAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Text = "You can upload an image or select one of the following choices",
                Buttons = new List<CardAction>
                {
                    new(
                        ActionTypes.ImBack,
                        "1. Inline Attachment",
                        value: "1"),
                    new(
                        ActionTypes.ImBack,
                        "2, Internet Attachment",
                        value:"2"),
                    new(
                        ActionTypes.ImBack,
                        "3, Uploaded Attachment",
                        value:"3"),
                },
            };

            var reply = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Given the input from the message, create the response.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<IMessageActivity> ProcessInput(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            IMessageActivity reply;

            // Activity has attachments
            if (activity.Attachments != null && activity.Attachments.Any())
            {
                // We know the user is sending an attachment as there is at least on item
                // in the attachments list.
                reply = HandleIncomingAttachment(activity);
            }
            else
            {
                // Send at attachment to the user.
                reply = await HandleOutgoingAttachment(turnContext, activity, cancellationToken);
            }

            return reply;
        }

        /// <summary>
        /// Outgoing Attachment Handler
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="activity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Message Activity</returns>
        private static async Task<IMessageActivity> HandleOutgoingAttachment(
            ITurnContext turnContext,
            Activity activity,
            CancellationToken cancellationToken)
        {
            IMessageActivity reply;
            
            if (activity.Text.StartsWith("1"))
            {
                reply = MessageFactory.Text("This is an inline attachment.");
                reply.Attachments = new List<Attachment>() {GetInlineAttachment()};
            }
            else if (activity.Text.StartsWith("2"))
            {
                reply = MessageFactory.Text("This is an attachment from a HTTP URL.");
                reply.Attachments = new List<Attachment>() {GetInternetAttachment()};
            }
            else if (activity.Text.StartsWith("3"))
            {
                reply = MessageFactory.Text("This is an uploaded attachment.");
                
                // Get the upload attachment.
                var uploadedAttachment = await GetUploadedAttachmentAsync(
                    turnContext,
                    activity.Conversation.Id,
                    cancellationToken);

                reply.Attachments = new List<Attachment>() { uploadedAttachment };
            }
            else
            {
                reply = MessageFactory.Text("Your input was not recognized please try again.");
            }

            return reply;
        }

        
        /// <summary>
        ///  Creates an "Attachment" to be sent from the bot to the user from an uploaded file.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="conversationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static async Task<Attachment> GetUploadedAttachmentAsync(
            ITurnContext turnContext,
            // string serviceUrl,
            string conversationId,
            CancellationToken cancellationToken)
        {
            // if (string.IsNullOrWhiteSpace(serviceUrl))
            //     throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));
            if (string.IsNullOrWhiteSpace(conversationId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(conversationId));

            var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.png");

            var connector = turnContext.TurnState.Get<IConnectorClient>() as ConnectorClient;
            var attachments = new Attachments(connector);
            var response = await attachments.Client.Conversations.UploadAttachmentAsync(
                conversationId,
                new AttachmentData
                {
                    Name = @"Resources\architecture-resize.png",
                    OriginalBase64 = await File.ReadAllBytesAsync(imagePath, cancellationToken),
                    Type = "image/png"
                },
                cancellationToken);

            var attachmentUri = attachments.GetAttachmentUri(response.Id);

            return new Attachment
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = attachmentUri
            };
        }

        /// <summary>
        /// Handle attachments uploaded by users.
        /// the bot receives an <see cref="Attachment" /> in an <see cref="Activity"/>.
        /// Not all channels allow users to upload files.
        /// Same channels have restrictions
        /// on file type, size, and other attributes.
        /// Consult the documentation for the channel for
        /// </summary>
        /// <param name="activity">
        /// Message Activity
        /// </param>
        /// <returns>Message with uploaded file name and saved path.</returns>
        private static IMessageActivity HandleIncomingAttachment(
            IMessageActivity activity)
        {
            var replyText = string.Empty;
            foreach (var file in activity.Attachments)
            {
                // Determine where the file is hosted.
                var remoteContentUrl = file.ContentUrl;
                
                // Save the attachment to the system temp directory.
                var localFileName = Path.Combine(Path.GetTempPath(), file.Name);
                
                // Download the actual attachment
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(remoteContentUrl, localFileName);
                }
                
                replyText += $"Attachment \"{file.Name}\"" +
                    $" has been received and saved to \"{localFileName}\"\r\n";
            }

            return MessageFactory.Text(replyText);
        }

        /// <summary>
        /// Creates an inline attachment sent from the bot to the user using a base64 string.
        /// 
        /// Using a base64 string to send an attachment will not work on all channels.
        /// Additionally, some channels will only allow certain file types to be sent this way.
        /// For example, a .png file may work but a .pdf file may not on some channels.
        /// </summary>
        /// <returns>
        /// Image Attachment
        /// </returns>
        private static Attachment GetInlineAttachment()
        {
            var imagePath = Path.Combine(
                Environment.CurrentDirectory,
                @"Resources",
                "architecture-resize.png");

            var imageData = Convert.ToBase64String(
                File.ReadAllBytes(imagePath));

            return new Attachment
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = $"data:image/png;base64,{imageData}",
            };
        }
        
        /// <summary>
        /// Get Internet Attachment
        /// </summary>
        /// <returns>
        ///Attachment
        /// - Name: file
        /// - Type: image/png
        /// - ContentUrl = URL(HTTPS)
        /// </returns>
        private static Attachment GetInternetAttachment()
        {
            // ContentUrl must be https.
            return new()
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png"
            };
        }
        
        /// <summary>
        /// When the user first calls the bot.
        /// </summary>
        /// <param name="membersAdded"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>welcome message</returns>
        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            const string welcomeText = "Hello and welcome!";
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

                    await DisplayOptionsAsync(turnContext, cancellationToken);
                }
            }
        }
        
        /// <summary>
        /// On message Activity
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var reply = await ProcessInput(turnContext, cancellationToken);
            await turnContext.SendActivityAsync("HI", cancellationToken: cancellationToken);
            // respond to the user
            await turnContext.SendActivityAsync(reply, cancellationToken);
            await DisplayOptionsAsync(turnContext, cancellationToken);
        }
    }
}