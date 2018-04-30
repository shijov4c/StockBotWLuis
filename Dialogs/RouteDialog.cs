using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.LuisBot;

namespace LuisBot.Dialogs
{
	[Serializable]
	public class RouteDialog : IDialog<object>
	{
		public Task StartAsync(IDialogContext context)
		{
			context.Wait(MessageReceivedAsync);

			return Task.CompletedTask;
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			string userName = "";
			bool userNameExists = context.UserData.TryGetValue("UserName", out userName);
			string resetStatus = "";
			bool resetExists = context.UserData.TryGetValue("reset", out resetStatus);

			if (!resetExists)
			{
				context.UserData.SetValue("reset", "");
			}

			if (activity.Text.ToLower().Equals("reset") || resetStatus.Equals("reset"))
			{
				context.UserData.SetValue("UserName", "");
				context.UserData.SetValue("reset", "");

				await context.PostAsync($"You have reset the data successfully. Please provide your name to re-start.");
				context.Wait(MessageReceivedAsync);
			}
			else if (activity.Text.ToLower().Equals("questions"))
			{
				var faqDialog = new BaseQnAMakerDialog();
				await context.Forward(faqDialog, AfterFAQDialog, activity, CancellationToken.None);
			}
			else if (activity.Text.ToLower().Equals("category"))
			{
				var faqDialog = new AzureSearchDialog();
				await context.Forward(faqDialog, AfterFAQDialog, activity, CancellationToken.None);
			}
			else
			{

				if (userNameExists && !(userName.Equals(string.Empty)))
				{
					// calculate something for us to return
					//int length = (activity.Text ?? string.Empty).Length;

					//// return our reply to the user
					//await context.PostAsync($"You sent {activity.Text} which was {length} characters");
					var faqDialog = new RootDialog();
					await context.Forward(faqDialog, AfterFAQDialog, activity, CancellationToken.None);
				}
				else
				{
					var faqDialog = new BasicLuisDialog();
					await context.Forward(faqDialog, AfterFAQDialog, activity, CancellationToken.None);
				}
			}
		}

		private async Task _reset(Activity activity)
		{
			await activity.GetStateClient().BotState
				.DeleteStateForUserWithHttpMessagesAsync(activity.ChannelId, activity.From.Id);

			var client = new ConnectorClient(new Uri(activity.ServiceUrl));
			var clearMsg = activity.CreateReply();
			clearMsg.Text = $"Reseting everything for conversation: {activity.Conversation.Id}";
			await client.Conversations.SendToConversationAsync(clearMsg);
		}

		private async Task AfterFAQDialog(IDialogContext context, IAwaitable<object> result)
		{
			var messageHandled = await result;
			//if (!messageHandled)
			//{
			//await context.PostAsync("Did that answer your question?");
			//}

			context.Wait(MessageReceivedAsync);
		}

		private async Task ResumeAfterSearchDialog(IDialogContext context, IAwaitable<object> result)
		{
			var messageHandled = await result;
			//if (!messageHandled)
			//{
			//await context.PostAsync("Did that answer your question?");
			//}

			context.Wait(MessageReceivedAsync);
		}
	}
}