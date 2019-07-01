using System.Collections.Generic;

namespace Zone.Epi.ContentCopy.Models
{
	public class ContentCopyRequestDetail
	{
		public string CultureCode { set; get; }

		public int Source { set; get; }

		public int Destination { set; get; }

		public IEnumerable<string> Properties { set; get; }

		public bool CopyAndPublishAutomatically { get; set; }
	}
}