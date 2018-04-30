using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;
namespace LuisBot.Dialogs
{
	[Serializable]
	public class BaseQnAMakerDialog : IDialog<object>
	{
		public async Task StartAsync(IDialogContext context)
		{
			var qnaSubscriptionKey = Utils.GetAppSetting("QnASubscriptionKey");
			var qnaKBId = Utils.GetAppSetting("QnAKnowledgebaseId");

			if (string.IsNullOrEmpty(qnaSubscriptionKey) || string.IsNullOrEmpty(qnaKBId))
			{
				await context.PostAsync("Please set up the QnASubscriptionKey and QnAKnowledgebaseId in the app settings");
			}
			/* Wait until the first message is received from the conversation and call MessageReceviedAsync 
			*  to process that message. */
			context.Wait(this.MessageReceivedAsync);
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			/* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
            *  await the result. */
			var message = await result;

			if (message.Text.Equals("questions"))
			{
				await context.PostAsync("Please ask your question in simple language format. Enter reset if you need to start over.");
				context.Wait(MessageReceivedAsync);
			}
			else if (message.Text.ToLower().Equals("reset"))
			{
				// return our reply to the user
				context.UserData.SetValue("reset", "reset");
				context.Call(new RouteDialog(), AfterFAQDialog);
				context.Wait(MessageReceivedAsync);
			}
			else
			{

				await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);

			}

		}

		private async Task AfterAnswerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			// wait for the next user message
			context.Wait(MessageReceivedAsync);
		}

		private async Task AfterFAQDialog(IDialogContext context, IAwaitable<object> result)
		{
			var messageHandled = await result;
			//if (!messageHandled)
			//{
			await context.PostAsync("Did that answer your question?");
			//}

			context.Wait(MessageReceivedAsync);
		}
	}

	// For more information about this template visit http://aka.ms/azurebots-csharp-qnamaker
	[Serializable]
	public class BasicQnAMakerDialog : QnAMakerDialog
	{
		// Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.        
		// Parameters to QnAMakerService are:
		// Required: subscriptionKey, knowledgebaseId, 
		// Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
		public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(Utils.GetAppSetting("QnASubscriptionKey"), Utils.GetAppSetting("QnAKnowledgebaseId"), "No good match in FAQ.", 0.5)))
		{ }
	}
}