using EoCPlugin;

using EoCPluginCSharp;

using HarmonyLib;

using LSFrameworkPlugin;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Runtime.InteropServices;
using Synchronization.Controls;

using LeaderTweaks.Win32.Constants;
using LeaderTweaks.Win32;
using LSToolFramework;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Animation Window Fixes")]
	public class AnimationWindowPatcher : IPatcher
	{
		static readonly Type t_AnimationPreviewToolPanel = typeof(AnimationPreviewToolPanel);
		static readonly Type t_AnimationsDialog = typeof(AnimationsDialog);


		public void Init(Harmony harmony)
		{
			var pt = typeof(AnimationWindowPatcher);

			harmony.Patch(AccessTools.Method(t_AnimationPreviewToolPanel, nameof(AnimationPreviewToolPanel.StoreInitAnim)),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.StoreInitAnimFix))));

			//Adding textkeys with right click fixes
			harmony.Patch(AccessTools.Method(t_AnimationPreviewToolPanel, "SetAnimationInternal"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.AnimationPreviewToolPanel_EnableTrackControls))));
			harmony.Patch(AccessTools.Method(t_AnimationPreviewToolPanel, nameof(AnimationPreviewToolPanel.RefreshControls)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.AnimationPreviewToolPanel_EnableTrackControls))));
			harmony.Patch(AccessTools.PropertyGetter(typeof(SynchTrackControl), nameof(SynchTrackControl.Changeable)),
				transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.t_SynchTrackControl_Changeable))));

			harmony.Patch(AccessTools.Method(t_AnimationsDialog, nameof(AnimationsDialog.HasDirtyAnimations)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.HasDirtyAnimations))));

			harmony.Patch(AccessTools.Method(t_AnimationsDialog, "InitializeComponent"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.TweakAnimationPreviewWindow))));
			harmony.Patch(AccessTools.Method(t_AnimationsDialog, "EnsureAnimationsPreviewPanelCreated"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.AnimationsDialog_DisableTopMost))));
			harmony.Patch(AccessTools.Method(typeof(ContentBrowser), "ResourceOpenHandler"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.ContentBrowser_AnimationsDialog_DisableTopMost))));

			var typeArgs = new Type[] { typeof(Dictionary<string, List<MAnimationDesc>>), typeof(VisualResource) };
			harmony.Patch(AccessTools.Method(t_AnimationsDialog, nameof(AnimationsDialog.Load), typeArgs),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.LoadDefaultPreviewVisual))));
		}

		static readonly FieldInfo f_m_ReadOnly = AccessTools.Field(t_AnimationPreviewToolPanel, "m_ReadOnly");
		static readonly FieldInfo f_m_AnimationControl = AccessTools.Field(t_AnimationPreviewToolPanel, "m_AnimationControl");

		// Fixes animations being set as "read only", which then prevents adding textkeys.
		public static void AnimationPreviewToolPanel_EnableTrackControls(SynchControl ___m_AnimationControl)
		{
			___m_AnimationControl.SetTrackControlsEnabled(true);
		}

		// Fixes animations being set as "read only", which then prevents adding textkeys.
		public static IEnumerable<CodeInstruction> t_SynchTrackControl_Changeable(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldc_I4_1);
			yield return new CodeInstruction(OpCodes.Ret);
		}

		/*
			System.NullReferenceException: Object reference not set to an instance of an object.
			at LSFrameworkPlugin.AnimationPreviewToolPanel.StoreInitAnim()
			at LSFrameworkPlugin.AnimationsDialog.AnimationsDialogFormClosing(Object A_0, FormClosingEventArgs A_1)
		 */
		public static bool StoreInitAnimFix(AnimationResource ___m_AnimationResource, AnimationResource ___m_InitAnimationResource)
		{
			if (___m_AnimationResource == null)
			{
				return false;
			}
			___m_AnimationResource.TextKeys.Clear();
			if (___m_InitAnimationResource?.TextKeys?.Count > 0)
			{
				foreach (var textkey in ___m_InitAnimationResource.TextKeys)
				{
					___m_AnimationResource.TextKeys.Add(textkey);
				}
			}

			___m_AnimationResource.Apply();
			//Disable original function
			return false;
		}

		// Fixes the window assuming there's changes for you to save, when there are none.
		public static void HasDirtyAnimations(ref bool __result, AnimationsDialog __instance,
			Dictionary<string, List<MAnimationDesc>> ___m_Animations, AnimationPreviewToolPanel ___m_AnimationPreviewToolPanel)
		{
			if (___m_AnimationPreviewToolPanel != null)
			{
				var m_AnimationResource = AccessTools.Field(typeof(AnimationPreviewToolPanel), "m_AnimationResource").GetValue(___m_AnimationPreviewToolPanel);
				var m_InitAnimationResource = AccessTools.Field(typeof(AnimationPreviewToolPanel), "m_InitAnimationResource").GetValue(___m_AnimationPreviewToolPanel);
				if (m_AnimationResource == null || m_InitAnimationResource == null)
				{
					__result = false;
					return;
				}
			}
		}

		static readonly FieldInfo f_m_AnimationPreviewPanel = AccessTools.Field(typeof(AnimationsDialog), "m_AnimationPreviewPanel");
		//static readonly FieldInfo f_m_Instance = AccessTools.Field(typeof(AnimationsDialog), "m_Instance");

		static readonly SetWindowPosFlags PreviewFlags = SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.DoNotReposition | SetWindowPosFlags.IgnoreResize;

		// Makes the window not stay on top of absolutely everything
		public static void TweakAnimationPreviewWindow(AnimationsDialog __instance)
		{
			__instance.TopMost = false;

			__instance.Activated += (s, e) =>
			{
				__instance.TopMost = false;

				//var previewPanel = f_m_AnimationPreviewPanel.GetValue(s);
				//if (previewPanel != null && previewPanel is DockContent m_AnimationPreviewPanel)
				//{
				//	m_AnimationPreviewPanel.TopMost = false;
				//}
			};
			//User32.SetWindowPos(__instance.Handle, HWND.NoTopMost, 0, 0, 0, 0, PreviewFlags);
		}

		public static void AnimationsDialog_DisableTopMost(AnimationsDialog __instance)
		{
			__instance.TopMost = false;
		}

		public static void ContentBrowser_AnimationsDialog_DisableTopMost(object A_0, EventArgs A_1)
		{
			//Get the private static variable since GetInstance() will create it otherwise.
			var instance = Traverse.Create(typeof(AnimationsDialog)).Field("m_Instance").GetValue() as AnimationsDialog;
			if (instance != null)
			{
				instance.TopMost = false;
			}
		}

		// Defaults to a proxymesh preview visual so you can see the timeline
		public static void LoadDefaultPreviewVisual(Dictionary<string, List<MAnimationDesc>> animDescMap, ref LSFrameworkPlugin.VisualResource visualResource, AnimationsDialog __instance)
		{
			if (visualResource == null)
			{
				//ProxyMesh_Humans_Hero_Male_Fullbody
				if (System.Guid.TryParse("11a0f5d4-f764-4644-bbb6-585e463a88c9", out var id))
				{
					visualResource = ResourceManager.Instance.GetResource(id) as LSFrameworkPlugin.VisualResource;
				}
			}
		}
	}
}
