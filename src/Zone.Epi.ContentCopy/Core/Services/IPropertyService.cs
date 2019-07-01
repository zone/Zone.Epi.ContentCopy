using Zone.Epi.ContentCopy.Models;

namespace Zone.Epi.ContentCopy.Core.Services
{
	public interface IPropertyService
	{
		ContentDetailItem GetContentDetail(int id, string languageCode, bool onlyCultureSpecific);

		ContentCopyResult CopyContentProperties(ContentCopyRequestDetail copyRequest);
	}
}
