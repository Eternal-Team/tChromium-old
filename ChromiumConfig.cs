using System.IO;
using Terraria;
using Terraria.ModLoader.Config;

namespace TChromiumFX
{
	public class ChromiumConfig : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

		private string DefaultCachePath => Path.Combine(Main.SavePath, "tChromium/Cache");
		private string DefaultResourcesPath => Path.Combine(Main.SavePath, "tChromium/Resources");
		private string DefaultCefCfxPath => Path.Combine(Main.SavePath, "tChromium/CefCfx");

		[Label("Path used for tChromium's cache")]
		public string CachePath
		{
			get => string.IsNullOrWhiteSpace(_cachePath) ? DefaultCachePath : _cachePath;
			set
			{
				try
				{
					Directory.CreateDirectory(CachePath);
					_cachePath = value;
				}
				catch
				{
					_cachePath = DefaultCachePath;
				}
			}
		}

		private string _cachePath;

		[Label("Path used for tChromium's resources/locales")]
		public string ResourcesPath
		{
			get => string.IsNullOrWhiteSpace(_resourcesPath) ? DefaultResourcesPath : _resourcesPath;
			set
			{
				try
				{
					Directory.CreateDirectory(ResourcesPath);
					_resourcesPath = value;
				}
				catch
				{
					_resourcesPath = DefaultResourcesPath;
				}
			}
		}

		private string _resourcesPath;

		[Label("Path used for tChromium's cef/cfx files")]
		public string CefCfxPath
		{
			get => string.IsNullOrWhiteSpace(_cefcfxPath) ? DefaultCefCfxPath : _cefcfxPath;
			set
			{
				try
				{
					Directory.CreateDirectory(CefCfxPath);
					_cefcfxPath = value;
				}
				catch
				{
					_cefcfxPath = DefaultCefCfxPath;
				}
			}
		}

		private string _cefcfxPath;

		public override void PostAutoLoad()
		{
			TChromiumFX.Config = this;
		}

		public override bool NeedsReload(ModConfig old)
		{
			if (!(old is ChromiumConfig chromiumConfig)) return base.NeedsReload(old);

			return chromiumConfig.CachePath != CachePath;
		}
	}
}