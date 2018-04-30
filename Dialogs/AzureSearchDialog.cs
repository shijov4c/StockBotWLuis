using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using LuisBot.Models;

namespace LuisBot.Dialogs
{
	[Serializable]
	public class AzureSearchDialog : IDialog<object>
	{
		string searchText = "";
		public Task StartAsync(IDialogContext context)
		{
			context.Wait(MessageReceivedAsync);

			return Task.CompletedTask;
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
		{
			var activity = await result as Activity;
			List<string> categories = new List<string> { "modes", "issues", "publish", "unknown" };

			if (activity.Text.ToLower().Equals("category"))
			{
				var replyCheckMode = ((Activity)context.Activity).CreateReply();
				var cardModes = new HeroCard
				{
					Title = "Choose your category",
					Text = $"Please select a category to search for content"
				};
				cardModes.Buttons = new List<CardAction>
				{
					new CardAction(ActionTypes.ImBack, "Modes", value: "modes"),
					new CardAction(ActionTypes.ImBack, "Issues", value: "issues"),
					new CardAction(ActionTypes.ImBack, "Publish", value: "publish"),
					new CardAction(ActionTypes.ImBack, "Dont know my category", value: "unknown"),
				};

				replyCheckMode.Attachments.Add(cardModes.ToAttachment());

				await context.PostAsync(replyCheckMode);

				context.Wait(MessageReceivedAsync);
			}
			else if (categories.Contains(activity.Text.ToLower()))
			{
				await context.PostAsync("Please enter a question to search the database");

				context.UserData.SetValue("srchcategory", activity.Text.ToLower());
			}
			else
			{

				string searchCateg = "";
				bool resetExists = context.UserData.TryGetValue("srchcategory", out searchCateg);

				ISearchIndexClient indexClientForQueries = CreateSearchIndexClient();

				// For more examples of calling search with SearchParameters, see
				// https://github.com/Azure-Samples/search-dotnet-getting-started/blob/master/DotNetHowTo/DotNetHowTo/Program.cs.  

				await context.PostAsync("Searching faq database...");

				if (searchCateg.Equals("unknown"))
				{
					DocumentSearchResult results = await indexClientForQueries.Documents.SearchAsync(activity.Text);
					await SendResults(context, results);
				}
				else
				{
					var parameters = new SearchParameters()
					{
						Filter = "category eq '" + searchCateg + "'"//,
																	//Top = 5
					};

					DocumentSearchResult results = await indexClientForQueries.Documents.SearchAsync(activity.Text, searchParameters: parameters);
					await SendResults(context, results);
				}
			}
		}

		private async Task SendResults(IDialogContext context, DocumentSearchResult results)
		{
			var message = context.MakeMessage();

			if (results.Results.Count == 0)
			{
				await context.PostAsync("There were no results found for \"" + searchText + "\".");
				context.Done<object>(null);
			}
			else
			{
				SearchHitStyler searchHitStyler = new SearchHitStyler();
				searchHitStyler.Apply(
					ref message,
					"Here are the results that I found:",
					results.Results.Select(r => ImageMapper.ToSearchHit(r)).ToList().AsReadOnly());

				await context.PostAsync(message);
				context.Done<object>(null);
			}
		}

		private ISearchIndexClient CreateSearchIndexClient()
		{
			string searchServiceName = ConfigurationManager.AppSettings["SearchDialogsServiceName"];
			string queryApiKey = ConfigurationManager.AppSettings["SearchDialogsServiceKey"];
			string indexName = ConfigurationManager.AppSettings["SearchDialogsIndexName"];

			SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
			return indexClient;
		}

		[Serializable]
		public class SearchHitStyler : PromptStyler
		{
			public void Apply<T>(ref IMessageActivity message, string prompt, IReadOnlyList<T> options, IReadOnlyList<string> descriptions = null)
			{
				var hits = options as IList<SearchHit>;
				if (hits != null)
				{
					var cards = hits.Select(h => new HeroCard
					{
						Title = h.Question,
						Text = h.Answer
					});

					message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
					message.Attachments = cards.Select(c => c.ToAttachment()).ToList();
					message.Text = prompt;
				}
				else
				{
					base.Apply<T>(ref message, prompt, options, descriptions);
				}
			}
		}
	}
}