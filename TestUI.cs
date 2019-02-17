using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;

namespace TChromiumFX
{
	public class TestUI : BaseUI
	{
		public override void OnInitialize()
		{
			const string google = "https://www.google.com";
			const string rickEmbed = "http://www.youtube.com/embed/DLzxrzFCyOs?autoplay=1";
			const string yes = "https://www.youtube.com/watch?v=sq_Fm7qfRQk";
			const string twitch = "https://www.twitch.tv/videos/381306498";
			const string discord = "https://discordapp.com/login";
			const string reddit = "https://www.reddit.com/r/Terraria_Mods/";

			UIDraggablePanel panel = new UIDraggablePanel
			{
				Width = (0, 0.5f),
				Height = (0, 0.6f),
				Position = new Vector2(20, 100)
			};

			UIText text = new UIText("Internet Explorer")
			{
				Position = new Vector2(8, 8)
			};
			panel.Append(text);

			UIWebBrowser browser = new UIWebBrowser(yes)
			{
				Width = (-16, 1),
				Height = (-44, 1),
				Position = new Vector2(8, 36)
			};
			panel.Append(browser);

			Append(panel);
		}
	}
}