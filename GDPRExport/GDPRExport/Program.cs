using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Configuration;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = DoExport().Result;
        }

        public static async Task<bool> DoExport()
        {
            // REPLACE with REAL APPID and PASSWORD
            var credentials = new MicrosoftAppCredentials("MsAppId", "MsAppPassword");
            StringBuilder outputMessage = new StringBuilder();
            string continuationToken = "";

            // REPLACE with bot framework API 
            var stateUrl = new Uri("https://intercom-api-scratch.azurewebsites.net"); 
            MicrosoftAppCredentials.TrustServiceUrl(stateUrl.AbsoluteUri);
            

            var client = new StateClient(stateUrl, credentials);
            var state = client.BotState;
            BotStateDataResult stateResult = null;
            do
            {
                try
                {
                    // should work with "directline", "facebook", or "kik"
                    stateResult = await BotStateExtensions.ExportBotStateDataAsync(state, "directline", continuationToken).ConfigureAwait(false);
                    foreach (var datum in stateResult.BotStateData)
                    {
                        outputMessage.Append($"conversationID: {datum.ConversationId}\tuserId: {datum.UserId}\tdata:{datum.Data}\n");
                        // If you were exporting into a new bot state store, here is where you would write the data
                        //if (string.IsNullOrEmpty(datum.ConversationId))
                        //{
                        //    // use SetUserData(datum.UserId, data.Data);
                        //}
                        //else
                        //{
                        //    SetPrivateConversationData(datum.UserId, datum.ConversationId, datum.Data);
                        //}
                    }
                    continuationToken = stateResult.ContinuationToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            } while (!string.IsNullOrEmpty(continuationToken));

            Console.WriteLine(outputMessage.ToString());


            continuationToken = null;
            outputMessage = new StringBuilder();
            // REPLACE with channel's URL.
            var connectionsUrl = new Uri("http://ic-directline-scratch.azurewebsites.net"); 
            MicrosoftAppCredentials.TrustServiceUrl(connectionsUrl.AbsoluteUri);
            var connectorClient = new ConnectorClient(connectionsUrl, credentials);
            var conversations = connectorClient.Conversations;
            ConversationsResult conversationResults = null;
            do
            {
                try
                {
                    conversationResults = await conversations.GetConversationsAsync(continuationToken).ConfigureAwait(false);
                    if (conversationResults == null)
                    {
                        outputMessage.Append("Internal error, conversation results was empty");
                    }
                    else if (conversationResults.Conversations == null)
                    {
                        outputMessage.Append("No conversations found for this bot in this channel");
                    }
                    else
                    {
                        outputMessage.Append($"Here is a batch of {conversationResults.Conversations.Count} conversations:\n");
                        foreach (var conversationMembers in conversationResults.Conversations)
                        {
                            string members = string.Join(", ",
                                conversationMembers.Members.Select(member => member.Id));
                            outputMessage.Append($"Conversation: {conversationMembers.Id} members: {members}\n");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                continuationToken = conversationResults?.Skip;  // should be ContinuationToken (this version is built on an old library
            } while (!string.IsNullOrEmpty(continuationToken));
            Console.WriteLine(outputMessage.ToString());

            return true;
        }
    }
}
