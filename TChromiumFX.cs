using System.Collections.Generic;
using System.IO;
using BaseLibrary.InputInterceptor;
using BaseLibrary.UI;
using BaseLibrary.Utility;
using Chromium;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace TChromiumFX
{
	public class TChromiumFX : Mod
	{
		public static string[] Titles = { "Internet Explorer", "Microsoft Edge", "Google Chrome", "Mozilla Firefox", "Opera" };

		public static TChromiumFX Instance;
		public static ChromiumConfig Config;

		public Effect effect;

		internal UIWebBrowser FocusedBrowser;

		public GUI<TestUI> GUI;

		public override void Load()
		{
			Instance = this;
			effect = GetEffect("Effects/BGRtoRGB");

			Scheduler.EnqueueMessage(() =>
			{
				Platform.Current.SetWindowUnicodeTitle(Main.instance.Window, Titles[Main.rand.Next(Titles.Length)]);

				if (!CfxRuntime.LibrariesLoaded)
				{
					string path = CfxRuntime.PlatformArch == CfxPlatformArch.x64 ? "x64" : "x86";
					CfxRuntime.LibCefDirPath = Path.Combine(Config.CefCfxPath, path);
					CfxRuntime.LibCfxDirPath = Path.Combine(Config.CefCfxPath, path);
				}

				var exitCode = CfxRuntime.ExecuteProcess();
				if (exitCode >= 0) typeof(ModLoader).InvokeMethod<object>("DisableMod", Name);

				var settings = new CfxSettings
				{
					WindowlessRenderingEnabled = true,
					NoSandbox = true,
					ResourcesDirPath = Config.ResourcesPath,
					LocalesDirPath = Path.Combine(Config.ResourcesPath, "locales"),
					CachePath = Config.CachePath
				};
				settings.MultiThreadedMessageLoop = true;

				CfxApp app = new CfxApp();
				app.OnBeforeCommandLineProcessing += (s, e) =>
				{
					e.CommandLine.AppendSwitch("single-process");
					e.CommandLine.AppendSwitch("disable-gpu");
					e.CommandLine.AppendSwitch("disable-gpu-compositing");
					e.CommandLine.AppendSwitch("disable-gpu-vsync");
					e.CommandLine.AppendSwitchWithValue("enable-media-stream", "1");
				};

				if (!CfxRuntime.Initialize(settings, app)) typeof(ModLoader).InvokeMethod<object>("DisableMod", Name);

				GUI = BaseLibrary.Utility.Utility.SetupGUI<TestUI>();
				GUI.Visible = true;
			});

			void UpdateMouseEvent()
			{
				FocusedBrowser.mouseEvent.Modifiers = Utility.GetModifiers();
				FocusedBrowser.mouseEvent.X = FocusedBrowser.RelativeMousePosition.X;
				FocusedBrowser.mouseEvent.Y = FocusedBrowser.RelativeMousePosition.Y;
			}

			InputInterceptor.InterceptInput += () => FocusedBrowser != null;

			InputInterceptor.OnMouseMove += (x, y, modifiers) =>
			{
				PlayerInput.MouseX = x;
				PlayerInput.MouseY = y;

				Main.lastMouseX = Main.mouseX;
				Main.lastMouseY = Main.mouseY;
				Main.mouseX = PlayerInput.MouseX;
				Main.mouseY = PlayerInput.MouseY;

				typeof(PlayerInput).SetValue("_originalMouseX", Main.mouseX);
				typeof(PlayerInput).SetValue("_originalMouseY", Main.mouseY);
				typeof(PlayerInput).SetValue("_originalLastMouseX", Main.lastMouseX);
				typeof(PlayerInput).SetValue("_originalLastMouseY", Main.lastMouseY);

				UpdateMouseEvent();
				FocusedBrowser.BrowserHost.SendMouseMoveEvent(FocusedBrowser.mouseEvent, false);
			};
			InputInterceptor.OnMouseWheel += (delta, modifiers) =>
			{
				PlayerInput.ScrollWheelValue = PlayerInput.ScrollWheelValueOld;

				bool shiftDown = (modifiers & 4) != 0;

				UpdateMouseEvent();
				FocusedBrowser.BrowserHost.SendMouseWheelEvent(FocusedBrowser.mouseEvent, shiftDown ? delta : 0, shiftDown ? 0 : delta);
			};

			InputInterceptor.OnLeftMouseDown += (clickCount, modifiers) =>
			{
				UpdateMouseEvent();
				FocusedBrowser.BrowserHost.SendMouseClickEvent(FocusedBrowser.mouseEvent, CfxMouseButtonType.Left, false, clickCount);
			};
			InputInterceptor.OnLeftMouseUp += (clickCount, modifiers) =>
			{
				UpdateMouseEvent();
				FocusedBrowser.BrowserHost.SendMouseClickEvent(FocusedBrowser.mouseEvent, CfxMouseButtonType.Left, true, clickCount);
			};

			InputInterceptor.OnRightMouseDown += (clickCount, modifiers) =>
			{
				UpdateMouseEvent();
				FocusedBrowser.BrowserHost.SendMouseClickEvent(FocusedBrowser.mouseEvent, CfxMouseButtonType.Right, false, clickCount);
			};
			InputInterceptor.OnRightMouseUp += (clickCount, modifiers) =>
			{
				UpdateMouseEvent();
				FocusedBrowser.BrowserHost.SendMouseClickEvent(FocusedBrowser.mouseEvent, CfxMouseButtonType.Right, true, clickCount);
			};

			InputInterceptor.OnMiddleMouseDown += (clickCount, modifiers) =>
			{
				UpdateMouseEvent();
				FocusedBrowser.BrowserHost.SendMouseClickEvent(FocusedBrowser.mouseEvent, CfxMouseButtonType.Middle, false, clickCount);
			};
			InputInterceptor.OnMiddleMouseUp += (clickCount, modifiers) =>
			{
				UpdateMouseEvent();
				FocusedBrowser.BrowserHost.SendMouseClickEvent(FocusedBrowser.mouseEvent, CfxMouseButtonType.Middle, true, clickCount);
			};

			InputInterceptor.OnKeyChar += (key, modifiers) =>
			{
				CfxKeyEvent keyEvent = new CfxKeyEvent
				{
					WindowsKeyCode = key,
					Type = CfxKeyEventType.Char,
					IsSystemKey = false,
					Modifiers = Utility.GetModifiers(),
					FocusOnEditableField = 1
				};

				FocusedBrowser.BrowserHost.SendKeyEvent(keyEvent);
			};
			InputInterceptor.OnKeyDown += (key, modifiers, systemKey) =>
			{
				CfxKeyEvent keyEvent = new CfxKeyEvent
				{
					WindowsKeyCode = key,
					Type = CfxKeyEventType.Keydown,
					IsSystemKey = systemKey,
					Modifiers = Utility.GetModifiers(),
					FocusOnEditableField = 1
				};

				FocusedBrowser.BrowserHost.SendKeyEvent(keyEvent);
			};
			InputInterceptor.OnKeyUp += (key, modifiers, systemKey) =>
			{
				CfxKeyEvent keyEvent = new CfxKeyEvent
				{
					WindowsKeyCode = key,
					Type = CfxKeyEventType.Keyup,
					IsSystemKey = systemKey,
					Modifiers = Utility.GetModifiers(),
					FocusOnEditableField = 1
				};

				FocusedBrowser.BrowserHost.SendKeyEvent(keyEvent);
			};
		}

		public override void Unload()
		{
			// bug: tell the user that terraria will get restarted
			CfxRuntime.Shutdown();
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int HotbarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));

			if (HotbarIndex != -1 && GUI != null && !Main.ingameOptionsWindow)
			{
				layers.Insert(HotbarIndex + 1, GUI.InterfaceLayer);
			}
			else FocusedBrowser = null;
		}

		public override void UpdateUI(GameTime gameTime)
		{
			GUI?.Update(gameTime);
		}
	}
}