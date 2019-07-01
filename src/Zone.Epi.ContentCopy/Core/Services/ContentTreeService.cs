using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using Zone.Epi.ContentCopy.Models;

namespace Zone.Epi.ContentCopy.Core.Services
{
	public class ContentTreeService : IContentTreeService
	{
		private readonly IContentLoader _contentLoader;

		public ContentTreeService(IContentLoader contentLoader)
		{
			_contentLoader = contentLoader;
		}

		public ContentTreeRootItem GetContentFamily(int rootId, string languageCode)
		{
			var page = _contentLoader.Get<PageData>(new ContentReference(rootId), CultureInfo.GetCultureInfo(languageCode));
			if (page == null)
			{
				return null;
			}

			var children = _contentLoader
				.GetChildren<PageData>(page.ContentLink, CultureInfo.GetCultureInfo(languageCode))
				.Select(s => new ContentTreeChildItem
				{
					Id = s.ContentLink.ToReferenceWithoutVersion().ID,
					Text = s.Name,
					Children = _contentLoader.GetChildren<PageData>(s.ContentLink, CultureInfo.GetCultureInfo(languageCode)).Any()
				});

			return new ContentTreeRootItem
			{
				Id = page.ContentLink.ToReferenceWithoutVersion().ID,
				Text = page.Name,
				Children = children
			};
		}
	}
}