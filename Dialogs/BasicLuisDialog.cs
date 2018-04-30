using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using LuisBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;


namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

		[LuisIntent("UserDetails")]
		public async Task NameIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
		{
			var msg = await message;
			//await context.PostAsync($"Hello!! How can I help you?");
			//context.Wait(MessageReceived);
			if (result.TopScoringIntent.Score > 0.5)
			{
				string entityValue = "";
				if (result.Entities.Count > 0)
				{
					foreach (EntityRecommendation item in result.Entities)
					{
						entityValue = item.Entity;
					}
				}
				string strRet = ""; // await QnABot.Models.Yahoo.GetStock(entityValue);

				// return our reply to the user
				context.UserData.SetValue("UserName", entityValue);
				//await context.PostAsync($"Hello {entityValue}!!. How can I help you?");
				var faqDialog = new RootDialog();
				await context.Forward(faqDialog, AfterFAQDialog, msg, CancellationToken.None);
				//context.Wait(MessageReceived);
			}
			else
			{
				await this.ShowLuisResult(context, result);
				context.Wait(MessageReceived);
			}
		}

		// Go to https://luis.ai and create a new intent, then train/publish your luis app.
		// Finally replace "Gretting" with the name of your newly created intent in the following handler
		[LuisIntent("Greeting")]
		public async Task GreetingIntent(IDialogContext context, LuisResult result)
		{
			await context.PostAsync($"Hello!! May I know your name please?");
			context.Wait(MessageReceived);
		}

		[LuisIntent("None")]
		public async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
		{
			await this.ShowLuisResult(context, result);
		}

		[LuisIntent("QnA")]
		public async Task QnAIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
		{
			await this.ShowLuisResult(context, result);
		}

		[LuisIntent("Cancel")]
		public async Task CancelIntent(IDialogContext context, LuisResult result)
		{
			await this.ShowLuisResult(context, result);
		}

		[LuisIntent("Help")]
		public async Task HelpIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
		{
			await this.ShowLuisResult(context, result);
		}

		[LuisIntent("StockPrice")]
		public async Task StockPriceIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
		{
			await this.ShowLuisResult(context, result);
		}

		private async Task ShowLuisResult(IDialogContext context, LuisResult result)
		{
			await context.PostAsync($"Sorry, I could not understand. Could you try another time? Please enter your name.");
			context.Wait(MessageReceived);
		}

		private async Task AfterFAQDialog(IDialogContext context, IAwaitable<object> result)
		{
			var messageHandled = await result;

			context.Done<object>(null);
		}
	}
}