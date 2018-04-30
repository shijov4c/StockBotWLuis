using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Models
{
	[Serializable]
	public class SearchHit
	{
		public SearchHit()
		{
			this.PropertyBag = new Dictionary<string, object>();
		}

		public string Key { get; set; }

		public string Question { get; set; }
		public string Answer { get; set; }

		public IDictionary<string, object> PropertyBag { get; set; }
	}
}