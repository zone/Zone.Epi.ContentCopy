using Zone.Epi.ContentCopy.Models;

namespace Zone.Epi.ContentCopy.Core.Services
{
	public interface IContentTreeService
	{
		ContentTreeRootItem GetContentFamily(int rootId, string languageCode);
	}
}