using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace Zone.Epi.ContentCopy.Core.Initialization
{
	[InitializableModule]
	public class CustomRouteInitialization : IInitializableModule
	{
		public void Initialize(InitializationEngine context)
		{
			RouteTable.Routes.MapRoute(
				 "Admin/ZoneContentCopy",
				 "Admin/ZoneContentCopy",
				 new { controller = "ContentCopy", action = "Index" });

			RouteTable.Routes.MapRoute(
				"Admin/ZoneContentCopy/GetAvailableLanguages",
				"Admin/ZoneContentCopy/GetAvailableLanguages",
				new { controller = "ContentCopy", action = "GetAvailableLanguages" });

			RouteTable.Routes.MapRoute(
				"Admin/ZoneContentCopy/GetContentChildren",
				"Admin/ZoneContentCopy/GetContentChildren",
				new { controller = "ContentCopy", action = "GetContentChildren" });

			RouteTable.Routes.MapRoute(
				"Admin/ZoneContentCopy/GetContentDetails",
				"Admin/ZoneContentCopy/GetContentDetails",
				new { controller = "ContentCopy", action = "GetContentDetails" });

			RouteTable.Routes.MapRoute(
				"Admin/ZoneContentCopy/SubmitCopyRequest",
				"Admin/ZoneContentCopy/SubmitCopyRequest",
				new { controller = "ContentCopy", action = "SubmitCopyRequest" });
		}

		public void Uninitialize(InitializationEngine context)
		{
		}
	}
}
