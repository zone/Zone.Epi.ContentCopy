using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using Zone.Epi.ContentCopy.Models;

namespace Zone.Epi.ContentCopy.Core.Services
{
	public class PropertyService : IPropertyService
	{
		private readonly IContentLoader _contentLoader;
		private readonly IContentRepository _contentRepository;
		private readonly IContentVersionRepository _contentVersionRepository;

		public PropertyService(IContentLoader contentLoader, IContentRepository contentRepository, IContentVersionRepository contentVersionRepository)
		{
			_contentLoader = contentLoader;
			_contentRepository = contentRepository;
			_contentVersionRepository = contentVersionRepository;
		}

		public ContentDetailItem GetContentDetail(int id, string languageCode, bool onlyCultureSpecific)
		{
			var content = _contentLoader.Get<PageData>(new ContentReference(id), CultureInfo.GetCultureInfo(languageCode));
			if (content == null)
			{
				return null;
			}

			var properties = content.Property.Where(x => x.IsPropertyData);
			if (onlyCultureSpecific)
			{
				properties = properties.Where(w => w.IsLanguageSpecific);
			}

			return new ContentDetailItem
			{
				Id = id,
				Text = content.Name,
				Properties = properties.OrderBy(x => x.Name).Select(x => x.Name).ToList()
			};
		}

		public ContentCopyResult CopyContentProperties(ContentCopyRequestDetail copyRequest)
		{
			// Validate user input
			if (copyRequest.Source == copyRequest.Destination)
			{
				return new ContentCopyResult
				{
					Status = "Failure. The 'Source' and 'Destination' page cannot be the same."
				};
			}

			if (copyRequest.Properties?.Any() != true)
			{
				return new ContentCopyResult
				{
					Status = "Failure. No properties selected to copy."
				};
			}

			var source = _contentLoader.Get<PageData>(new ContentReference(copyRequest.Source), CultureInfo.GetCultureInfo(copyRequest.CultureCode));

			var destination = _contentLoader.Get<PageData>(new ContentReference(copyRequest.Destination), CultureInfo.GetCultureInfo(copyRequest.CultureCode))?.CreateWritableClone();

			// Ensure the destination page can be updated
			var hasDraft = ContentHasOutstandingDraft(copyRequest.Destination, copyRequest.CultureCode);
			if (hasDraft && !copyRequest.CopyAndPublishAutomatically)
			{
				return new ContentCopyResult
				{
					Status = "Failure. The destination page already has a draft pending publish. Review and publish it manually or use the 'Publish Automatically' mode."
				};
			}

			// Copy all selected properties across
			foreach (var property in copyRequest.Properties)
			{
				var sourceProp = source.Property.FirstOrDefault(w => w.Name == property);
				var destinationProp = destination.Property.FirstOrDefault(w => w.Name == property);

				if (destinationProp?.Type == null)
				{
					return new ContentCopyResult
					{
						Status = $"Failure. The destination page does not have a {sourceProp.Name} property."
					};
				}

				if (sourceProp.Type != destinationProp.Type)
				{
					return new ContentCopyResult
					{
						Status = $"Failure. The source and destination {sourceProp.Name} are different content types."
					};
				}

				destinationProp.Value = sourceProp.Value;
			}

			var saved = _contentRepository.Save(destination, copyRequest.CopyAndPublishAutomatically ? SaveAction.Publish : SaveAction.CheckOut, AccessLevel.Edit);

			return new ContentCopyResult
			{
				Success = true,
				Status = "Content copied."
			};
		}

		private bool ContentHasOutstandingDraft(int contentId, string language)
		{
			var commonDraft = _contentVersionRepository.LoadCommonDraft(new ContentReference(contentId), language);
			return commonDraft.Status != VersionStatus.Published;
		}
	}
}