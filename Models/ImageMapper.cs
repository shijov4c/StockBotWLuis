using Microsoft.Azure.Search.Models;

namespace LuisBot.Models
{
	public class ImageMapper
	{
		public static SearchHit ToSearchHit(SearchResult hit)
		{
			var searchHit = new SearchHit
			{
				Key = (string)hit.Document["id"],
				Question = (string)hit.Document["question"],
				Answer = (string)hit.Document["answer"]
			};

			return searchHit;
		}
	}
}