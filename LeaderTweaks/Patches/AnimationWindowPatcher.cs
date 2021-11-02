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

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Animation Window Fixes")]
	public class AnimationWindowPatcher : IPatcher
	{
		public void Init(Harmony harmony)
		{
			var pt = typeof(AnimationWindowPatcher);

			harmony.Patch(AccessTools.Method(typeof(AnimationPreviewToolPanel), nameof(AnimationPreviewToolPanel.StoreInitAnim)),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.StoreInitAnimFix))));

			harmony.Patch(AccessTools.Method(typeof(AnimationsDialog), nameof(AnimationsDialog.HasDirtyAnimations)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(AnimationWindowPatcher.HasDirtyAnimations))));
		}

		/*
			System.NullReferenceException: Object reference not set to an instance of an object.
			at LSFrameworkPlugin.AnimationPreviewToolPanel.StoreInitAnim()
			at LSFrameworkPlugin.AnimationsDialog.AnimationsDialogFormClosing(Object A_0, FormClosingEventArgs A_1)
		 */
		//Shorten the Eyes of a Child text
		public static bool StoreInitAnimFix(AnimationResource ___m_AnimationResource, AnimationResource ___m_InitAnimationResource)
		{
			if(___m_AnimationResource == null)
			{
				return false;
			}
			___m_AnimationResource.TextKeys.Clear();
			if(___m_InitAnimationResource?.TextKeys?.Count > 0)
			{
				foreach(var textkey in ___m_InitAnimationResource.TextKeys)
				{
					___m_AnimationResource.TextKeys.Add(textkey);
				}
			}

			___m_AnimationResource.Apply();
			//Disable original function
			return false;
		}

		//Shorten the Eyes of a Child text
		public static void HasDirtyAnimations(ref bool __result, AnimationsDialog __instance, 
			Dictionary<string, List<MAnimationDesc>> ___m_Animations, AnimationPreviewToolPanel ___m_AnimationPreviewToolPanel)
		{
			if(___m_AnimationPreviewToolPanel != null)
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
	}
}
