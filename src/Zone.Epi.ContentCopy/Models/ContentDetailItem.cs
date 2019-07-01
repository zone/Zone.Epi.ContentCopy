using System.Collections.Generic;

namespace Zone.Epi.ContentCopy.Models
{
	public class ContentDetailItem
	{
		public int Id { get; set; }

		public string Text { get; set; }

		public IEnumerable<string> Properties { get; set; }
	}
}