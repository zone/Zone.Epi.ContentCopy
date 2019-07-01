using System.Collections.Generic;

namespace Zone.Epi.ContentCopy.Models
{
	public class ContentTreeRootItem : ContentTreeItem
	{
		public IEnumerable<ContentTreeChildItem> Children { get; set; }
	}
}