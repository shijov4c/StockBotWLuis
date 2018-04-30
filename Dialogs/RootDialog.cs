using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace LuisBot.Dialogs
{
	[Serializable]
	public class RootDialog : IDialog<object>
	{
		public Task StartAsync(IDialogContext context)
		{
			context.Wait(MessageReceivedAsync);

			return Task.CompletedTask;
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;

			var image = "https://assets.windowsphone.com/5cc1895e-5148-4700-856f-eb49d4bf28dc/Smarter_448x252_InvariantCulture_Default.png[";
			var reply = ((Activity)context.Activity).CreateReply();
			var card = new HeroCard
			{
				Title = "DOF FAQ Bot",
				Text = $"Welcome to DoF, {activity.Text}!"
			};
			card.Images.Add(new CardImage(image));

			reply.Attachments.Add(card.ToAttachment());

			var replyCheckMode = ((Activity)context.Activity).CreateReply();
			var cardModes = new HeroCard
			{
				Title = "Choose your search mode",
				Text = $"Please select a method to search for content"
			};
			cardModes.Buttons = new List<CardAction>
			{
				new CardAction(ActionTypes.ImBack, "Select categories to search FAQ", value: "category"),
				new CardAction(ActionTypes.ImBack, "Ask a question now", value: "questions"),
			};

			replyCheckMode.Attachments.Add(cardModes.ToAttachment());

			await context.PostAsync(reply);
			await context.PostAsync(replyCheckMode);

			context.Done<object>(null);

		}
	}
}