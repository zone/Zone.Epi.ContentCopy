using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.DataAbstraction;
using Zone.Epi.ContentCopy.Models;

namespace Zone.Epi.ContentCopy.Core.Services
{
	public class LanguageService : ILanguageService
	{
		private readonly ILanguageBranchRepository _languageBranchRepository;

		public LanguageService(ILanguageBranchRepository languageBranchRepository)
		{
			_languageBranchRepository = languageBranchRepository;
		}

		public IEnumerable<LanguageBranchItem> GetActiveLanguageBranches()
		{
			var textInfo = CultureInfo.InvariantCulture.TextInfo;

			return _languageBranchRepository.ListEnabled()?.Select(s => new LanguageBranchItem
			{
				Name = textInfo.ToTitleCase(s.Name),
				CultureCode = s.LanguageID
			});
		}
	}
}