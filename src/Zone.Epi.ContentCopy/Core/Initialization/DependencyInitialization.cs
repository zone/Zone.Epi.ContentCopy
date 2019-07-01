using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Zone.Epi.ContentCopy.Core.Services;

namespace Zone.Epi.ContentCopy.Core.Initialization
{
	[InitializableModule]
	public class DependencyInitialization : IConfigurableModule
	{
		public void ConfigureContainer(ServiceConfigurationContext context)
		{
			context.Services.AddTransient<IContentTreeService, ContentTreeService>();
			context.Services.AddTransient<ILanguageService, LanguageService>();
			context.Services.AddTransient<IPropertyService, PropertyService>();
		}

		public void Initialize(InitializationEngine context)
		{
		}

		public void Uninitialize(InitializationEngine context)
		{
		}
	}
}