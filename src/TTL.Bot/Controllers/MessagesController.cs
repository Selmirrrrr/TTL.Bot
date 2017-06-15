namespace TTL.Bot.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message) await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            else HandleSystemMessage(activity);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private void HandleSystemMessage(Activity message)
        {
            if (message.Type != ActivityTypes.ConversationUpdate) return;
            IConversationUpdateActivity update = message;
            var client = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
            if (update.MembersAdded == null || !update.MembersAdded.Any()) return;
            foreach (var newMember in update.MembersAdded)
            {
                if (newMember.Id == message.Recipient.Id) continue;
                var reply = message.CreateReply();
                reply.Text = $"Welcome {newMember.Name}! Enter a station name to get next 10 departures board.";
                client.Conversations.ReplyToActivityAsync(reply);
            }
        }
    }
}