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

			harmony.Patch(AccessTools.Method(t_AnimationPreviewToolPanel, "InitializeComponent"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.AnimationPreviewToolPanelInitialize))));

			harmony.Patch(AccessTools.PropertyGetter(typeof(SynchTrackControl), nameof(SynchTrackControl.Changeable)),
				transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.t_SynchTrackControl_Changeable))));
			harmony.Patch(AccessTools.Method(typeof(SynchTrackControl), "ElementTrackControl_MouseDown"),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.SynchTrackControl_ElementTrackControl_MouseDown))));

			harmony.Patch(AccessTools.Method(t_AnimationsDialog, nameof(AnimationsDialog.HasDirtyAnimations)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.HasDirtyAnimations))));

			harmony.Patch(AccessTools.Method(t_AnimationsDialog, "InitializeComponent"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.TweakAnimationPreviewWindow))));

			var typeArgs = new Type[] { typeof(Dictionary<string, List<MAnimationDesc>>), typeof(VisualResource) };
			harmony.Patch(AccessTools.Method(t_AnimationsDialog, nameof(AnimationsDialog.Load), typeArgs),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.LoadDefaultPreviewVisual))));
		}

		static readonly FieldInfo f_m_ReadOnly = AccessTools.Field(t_AnimationPreviewToolPanel, "m_ReadOnly");

		// Fixes animations being set as "read only", which then prevents adding textkeys.
		public static void AnimationPreviewToolPanelInitialize(AnimationPreviewToolPanel __instance)
		{
			f_m_ReadOnly.SetValue(__instance, false);
			Helper.Log($"AnimationPreviewToolPanel.m_ReadOnly = {f_m_ReadOnly.GetValue(__instance)}");
		}

		// Fixes animations being set as "read only", which then prevents adding textkeys.
		public static IEnumerable<CodeInstruction> t_SynchTrackControl_Changeable(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ldc_I4_1);
			yield return new CodeInstruction(OpCodes.Ret);
		}


		// Fixes animations being set as "read only", which then prevents adding textkeys.
		public static void SynchTrackControl_ElementTrackControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e, SynchTrackControl __instance)
		{
			Helper.Log($"SynchTrackControl.Changeable = {__instance.Changeable}");
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

		private static readonly IntPtr HWND_BOTTOM = new IntPtr(0);
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
		private const UInt32 SWP_NOSIZE = 0x0001;
		private const UInt32 SWP_NOMOVE = 0x0002;
		private const UInt32 WIN_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		// Makes the window not stay on top of absolutely everything
		public static void TweakAnimationPreviewWindow(AnimationsDialog __instance)
		{
			__instance.Activated += (s, e) =>
			{
				__instance.TopMost = false;
			};
			__instance.LostFocus += (s, e) =>
			{
				SetWindowPos(__instance.Handle, HWND_BOTTOM, 0, 0, 0, 0, WIN_FLAGS);
				__instance.SendToBack();
			};
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
