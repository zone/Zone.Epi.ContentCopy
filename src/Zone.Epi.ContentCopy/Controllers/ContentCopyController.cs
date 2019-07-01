using System.Web.Mvc;
using EPiServer.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Zone.Epi.ContentCopy.Core.Services;
using Zone.Epi.ContentCopy.Models;

namespace Zone.Epi.ContentCopy.Controllers
{
	[Authorize(Roles = "WebEditors, WebAdmins, Administrators")]
	public class ContentCopyController : Controller
	{
		private readonly IContentTreeService _contentTreeService;
		private readonly ILanguageService _languageService;
		private readonly IPropertyService _propertyService;

		public ContentCopyController(IContentTreeService contentTreeService, ILanguageService languageService, IPropertyService propertyService)
		{
			_contentTreeService = contentTreeService;
			_languageService = languageService;
			_propertyService = propertyService;
		}

		public ActionResult Index()
		{
			return PartialView(Paths.ToClientResource("Zone.Epi.ContentCopy", "Views") + "/Index.cshtml");
		}

		public ContentResult GetAvailableLanguages()
		{
			var languages = _languageService.GetActiveLanguageBranches();

			return CamelCaseJson(languages);
		}

		public ContentResult GetContentChildren(int id, string lang = "en")
		{
			// This is a special case as the root is only in English
			if (id == 1)
			{
				lang = "en";
			}

			var tree = _contentTreeService.GetContentFamily(id, lang);

			return CamelCaseJson(tree);
		}

		public ContentResult GetContentDetails(int id, string lang = "en", bool onlyCultureSpecific = false)
		{
			// This is a special case as the root is only in English
			if (id == 1)
			{
				lang = "en";
			}

			var details = _propertyService.GetContentDetail(id, lang, onlyCultureSpecific);

			return CamelCaseJson(details);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ContentResult SubmitCopyRequest(ContentCopyRequestDetail model)
		{
			var copyResult = _propertyService.CopyContentProperties(model);

			return CamelCaseJson(copyResult);
		}

		private ContentResult CamelCaseJson(object data)
		{
			var camelCaseFormatter = new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
			var json = JsonConvert.SerializeObject(data, camelCaseFormatter);

			return Content(json, "application/json");
		}
	}
}