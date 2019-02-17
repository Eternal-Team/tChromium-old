using System;
using System.Runtime.InteropServices;
using System.Threading;
using BaseLibrary.UI.Elements;
using Chromium;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace TChromiumFX
{
	// BUG: ?vq=hd720

	public class UIWebBrowser : UIWebBrowserBase
	{
		public string URL;

		private byte[] arr;
		private Texture2D texture;

		public Point RelativeMousePosition => (Main.MouseScreen - GetDimensions().Position()).ToPoint();

		public UIWebBrowser(string URL = "about:blank")
		{
			this.URL = URL;

			Register();

			OnSizeChanged += size =>
			{
				BrowserHost?.WasResized();

				int width = (int)size.Width, height = (int)size.Height;
				arr = new byte[width * height * 4];
				texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
			};
			renderHandler.OnPaint += (sender, args) => { Marshal.Copy(args.Buffer, arr, 0, args.Width * args.Height * 4); };
		}

		private void Register()
		{
			lifeSpanHandler = new CfxLifeSpanHandler();
			renderHandler = new CfxRenderHandler();
			loadHandler = new CfxLoadHandler();
			client = new CfxClient();
			windowInfo = new CfxWindowInfo();
			browserSettings = new CfxBrowserSettings { WindowlessFrameRate = 60 };
			mouseEvent = new CfxMouseEvent();

			lifeSpanHandler.OnAfterCreated += (sender, args) =>
			{
				browser = args.Browser;
				Thread.Sleep(100);
			};

			renderHandler.GetViewRect += (sender, args) =>
			{
				CalculatedStyle dimensions = GetDimensions();
				args.Rect.X = 0;
				args.Rect.Y = 0;
				args.Rect.Width = (int)dimensions.Width;
				args.Rect.Height = (int)dimensions.Height;
				args.SetReturnValue(true);
			};
			renderHandler.GetScreenPoint += (sender, args) =>
			{
				args.ScreenX = 0;
				args.ScreenY = 0;
				args.SetReturnValue(true);
			};

			loadHandler.OnLoadError += (sender, args) =>
			{
				if (args.ErrorCode == CfxErrorCode.Aborted)
				{
					var url = args.FailedUrl;
					var frame = args.Frame;
					ThreadPool.QueueUserWorkItem(state =>
					{
						Thread.Sleep(200);
						frame.LoadUrl(url);
					});
				}
			};

			client.GetLifeSpanHandler += (sender, args) => args.SetReturnValue(lifeSpanHandler);
			client.GetRenderHandler += (sender, args) => args.SetReturnValue(renderHandler);
			client.GetLoadHandler += (sender, args) => args.SetReturnValue(loadHandler);

			windowInfo.SetAsWindowless(IntPtr.Zero);

			CfxBrowserHost.CreateBrowser(windowInfo, client, URL, browserSettings, null);
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			TChromiumFX.Instance.FocusedBrowser = this;
			BrowserHost.SetFocus(true);
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			BrowserHost.SendMouseClickEvent(mouseEvent, CfxMouseButtonType.Left, true, 1);
			BrowserHost.SendMouseMoveEvent(mouseEvent, true);

			TChromiumFX.Instance.FocusedBrowser = null;
			BrowserHost.SetFocus(false);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (ContainsPoint(Main.MouseScreen))
			{
				BaseLibrary.BaseLibrary.InUI = true;
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
				Main.mouseText = false;
			}
		}

		public void LoadURL(string url)
		{
			URL = url;
			//this.PushMessage(MessageTypes.LOAD_URL, url);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			texture.SetData(arr);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

			TChromiumFX.Instance.effect.CurrentTechnique.Passes[0].Apply();
			Main.spriteBatch.Draw(texture, GetDimensions().Position(), Color.White);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin();
		}
	}

	public class UIWebBrowserBase : BaseElement
	{
		public CfxBrowser browser;
		public CfxBrowserHost BrowserHost => browser?.Host;

		public CfxClient client;
		public CfxWindowInfo windowInfo;
		public CfxBrowserSettings browserSettings;

		public CfxLifeSpanHandler lifeSpanHandler;
		public CfxRenderHandler renderHandler;
		public CfxLoadHandler loadHandler;

		public CfxMouseEvent mouseEvent;
	}
}