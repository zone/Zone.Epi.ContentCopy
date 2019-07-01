using System.Collections.Generic;
using Zone.Epi.ContentCopy.Models;

namespace Zone.Epi.ContentCopy.Core.Services
{
	public interface ILanguageService
	{
		IEnumerable<LanguageBranchItem> GetActiveLanguageBranches();
	}
}