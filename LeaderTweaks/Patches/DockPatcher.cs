using EoCPlugin;

using EoCPluginCSharp;

using HarmonyLib;

using LSCSharpCore.Controls;

using LSToolFramework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Docking Window Crap Fixes")]
	public class DockPatcher : IPatcher
	{
		public void Init(Harmony harmony)
		{
			var pt = typeof(DockPatcher);

			//harmony.Patch(AccessTools.Constructor(DockContentDescriptionType, new Type[] { typeof(FrameworkElement), typeof(string) }),
			//	postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(DockPatcher.DockContentDescriptionConstructor))));
			//harmony.Patch(AccessTools.Constructor(DockContentDescriptionType, new Type[] { typeof(DockContentControl), typeof(string) }),
			//	postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(DockPatcher.DockContentDescriptionConstructor))));

			var ps = typeof(PanelService);

			harmony.Patch(AccessTools.Method(ps, nameof(PanelService.SetTitle)),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(DockPatcher.OnPanelServiceSetTitle))));

			//harmony.Patch(AccessTools.Method(ps, nameof(PanelService.AddPanel), 
			//	new Type[] { typeof(FrameworkElement), typeof(string), typeof(EDockState) }),
			//	prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(DockPatcher.OnPanelServiceAddPanel))));
			//harmony.Patch(AccessTools.Method(ps, nameof(PanelService.AddPanel), 
			//	new Type[] { typeof(DockContentControl), typeof(EDockState) }),
			//	prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(DockPatcher.OnPanelServiceAddPanel2))));
			//harmony.Patch(AccessTools.Method(ps, nameof(PanelService.AddNonUniquePanel)),
			//	prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(DockPatcher.OnPanelServiceAddNonUniquePanel))));
			//harmony.Patch(AccessTools.Method(ps, "AddPanelInternal"),
			//	prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(DockPatcher.OnPanelServiceAddPanelInternal))));
		}

		//Shorten the Eyes of a Child text
		public static void OnPanelServiceSetTitle(FrameworkElement content, ref string title)
		{
			if (title.Contains("Eyes of"))
			{
				title = "Game";
			}
		}

		public static void DockContentDescriptionConstructor(object content, ref string title)
		{
			Helper.Log($"DockContentDescriptionConstructor: {title}");
		}

		public static void OnPanelServiceAddPanel(FrameworkElement panel, ref string title, EDockState dockState,
			PanelService __instance)
		{
			Helper.Log($"OnPanelServiceAddPanel: {title}");
		}

		public static void OnPanelServiceAddPanel2(DockContentControl panel, EDockState dockState,
			PanelService __instance)
		{
			Helper.Log($"OnPanelServiceAddPanel2: {panel.Text}");
		}

		public static void OnPanelServiceAddNonUniquePanel(DockContentControl panel, EDockState dockState)
		{
			Helper.Log($"OnPanelServiceAddNonUniquePanel: {panel.Text}");
		}

		public static readonly Type DockContentDescriptionType = AccessTools.TypeByName("LSToolFramework.PanelService+DockContentDescription");

		class DockContentDescription
		{
			static readonly MethodInfo m_Content = AccessTools.PropertyGetter(DockContentDescriptionType, "Content");
			static readonly MethodInfo m_Title = AccessTools.PropertyGetter(DockContentDescriptionType, "Title");
			static readonly MethodInfo m_HideOnClose = AccessTools.PropertyGetter(DockContentDescriptionType, "HideOnClose");
			static readonly MethodInfo m_Width = AccessTools.PropertyGetter(DockContentDescriptionType, "Width");
			static readonly MethodInfo m_Height = AccessTools.PropertyGetter(DockContentDescriptionType, "Height");
			static readonly MethodInfo m_WrapWinformsControl = AccessTools.Method(DockContentDescriptionType, "WrapWinformsControl");

			static readonly FastInvokeHandler h_Content = HarmonyLib.MethodInvoker.GetHandler(m_Content);
			static readonly FastInvokeHandler h_Title = HarmonyLib.MethodInvoker.GetHandler(m_Title);
			static readonly FastInvokeHandler h_HideOnClose = HarmonyLib.MethodInvoker.GetHandler(m_HideOnClose);
			static readonly FastInvokeHandler h_Width = HarmonyLib.MethodInvoker.GetHandler(m_Width);
			static readonly FastInvokeHandler h_Height = HarmonyLib.MethodInvoker.GetHandler(m_Height);
			static readonly FastInvokeHandler h_WrapWinformsControl = HarmonyLib.MethodInvoker.GetHandler(m_WrapWinformsControl);

			public FrameworkElement Content => (FrameworkElement)h_Content.Invoke(this.Source, null);
			public string Title => (string)h_Title.Invoke(this.Source, null);
			public bool HideOnClose => (bool)h_HideOnClose.Invoke(this.Source, null);
			public int Width => (int)h_Width.Invoke(this.Source, null);
			public int Height => (int)h_Height.Invoke(this.Source, null);

			public void WrapWinformsControl(Control control)
			{
				h_WrapWinformsControl.Invoke(Source, control);
			}

			//public CanCloseDelegate CanClose { get; }
			//public OnClosedDelegate OnClosed { get; }
			//public delegate bool CanCloseDelegate();
			//public delegate void OnClosedDelegate();

			public object Source { get; private set; }

			public DockContentDescription(object dockContentDescription)
			{
				this.Source = dockContentDescription;
			}

			public override string ToString()
			{
				return String.Join("\n\t", new string[] { $"Title: {Title}", $"Width: {Width}", $"Height: {Height}", $"HideOnClose: {HideOnClose}" });
			}

			//public DockContentDescription(DockContentControl content, string title)
			//{
			//	this.Content = this.WrapWinformsControl(content);
			//	this.Content.Name = content.GetType().FullName.Replace(".", "");
			//	this.Title = title;
			//	this.CanClose = new PanelService.DockContentDescription.CanCloseDelegate(content.CanClose);
			//	this.OnClosed = new PanelService.DockContentDescription.OnClosedDelegate(content.OnClose);
			//	this.HideOnClose = content.HideOnClose;
			//	this.Width = content.Width;
			//	this.Height = content.Height;
			//}

			//private WindowsFormsHost WrapWinformsControl(Control control)
			//{
			//	control.Dock = DockStyle.Fill;
			//	return new WindowsFormsHost
			//	{
			//		Child = control,
			//		VerticalAlignment = VerticalAlignment.Stretch,
			//		HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
			//	};
			//}
		}

		//Shorten the Eyes of a Child text
		public static void OnPanelServiceAddPanelInternal(object dockContentDescription, EDockState dockState,
			PanelService __instance, Dictionary<string, FrameworkElement> ___m_FrameworkElements)
		{
			//DockContentDescription desc = new DockContentDescription(dockContentDescription);
			//Helper.Log($"OnPanelServiceAddPanelInternal:\n{desc}");
			//if (!__instance.m_FrameworkElements.ContainsKey(dockContentDescription.Content.Name))
			//{
			//	__instance.AddToLayout(dockContentDescription, dockState);
			//	__instance.m_FrameworkElements.Add(dockContentDescription.Content.Name, dockContentDescription.Content);
			//	__instance.m_DockContentDescriptionInfos.Add(new PanelService.DockContentDescriptionInfo
			//	{
			//		Description = dockContentDescription,
			//		DockState = dockState
			//	});
			//}
		}
	}
}
