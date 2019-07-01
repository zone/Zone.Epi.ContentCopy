using EPiServer.PlugIn;

namespace Zone.Epi.ContentCopy
{
	[GuiPlugIn(
		Area = PlugInArea.AdminMenu,
		SortIndex = -1000,
		Url = "/Admin/ZoneContentCopy",
		DisplayName = "Content Copy")]
	public class ContentCopy
	{
	}
}