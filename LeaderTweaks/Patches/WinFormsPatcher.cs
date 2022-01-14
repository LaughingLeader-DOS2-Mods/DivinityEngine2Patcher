using EoCPlugin;

using EoCPluginCSharp;

using HarmonyLib;

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
	[LeaderPatcher("Window Tweaks", Enabled = false)]
	public class WinFormsPatcher : IPatcher
	{
		public static System.Windows.Forms.AutoScaleMode ScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		public static System.Drawing.Font DefaultFont;
		public static System.Drawing.SizeF ScaleDimensions = new System.Drawing.SizeF(6F, 13F);

		static Type cType = typeof(System.Windows.Forms.ContainerControl);
		public void Init(Harmony harmony)
		{
			var pt = typeof(WinFormsPatcher);
			var t2 = typeof(System.Windows.Forms.Form);

			try
			{
				DefaultFont = System.Drawing.SystemFonts.GetFontByName("MS Sans Serif");
			}
			catch (Exception ex)
			{
				Helper.Log($"{ex}");
			}

			if (DefaultFont == null) DefaultFont = System.Drawing.SystemFonts.DefaultFont;

			harmony.Patch(AccessTools.Constructor(cType, new Type[] { }),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(WinFormsPatcher.Constructor_SetScaleMode))));
			//harmony.Patch(AccessTools.Constructor(t2, new Type[] { }),
			//	postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(WinFormsPatcher.Form_SetFont))));
		}

		static readonly FieldInfo cType_autoScaleMode = AccessTools.Field(cType, "autoScaleMode");
		static readonly FieldInfo cType_autoScaleDimensions = AccessTools.Field(cType, "autoScaleDimensions");
		static readonly FieldInfo cType_currentAutoScaleDimensions = AccessTools.Field(cType, "currentAutoScaleDimensions");
		static readonly MethodInfo cType_OnAutoScaleModeChanged = AccessTools.Method(cType, "OnAutoScaleModeChanged", null, null);
		static readonly MethodInfo cType_LayoutScalingNeeded = AccessTools.Method(cType, "LayoutScalingNeeded", null, null);

		public static void Constructor_SetScaleMode(System.Windows.Forms.ContainerControl __instance)
		{
			//Helper.Log($"Setting AutoScaleMode from '{__instance.AutoScaleMode}' to '{ScaleMode}'.");
			cType_autoScaleMode?.SetValue(__instance, ScaleMode);
			cType_autoScaleDimensions?.SetValue(__instance, ScaleDimensions);
			cType_currentAutoScaleDimensions?.SetValue(__instance, ScaleDimensions);
			//__instance.AutoScaleMode = ScaleMode;
			__instance.HandleCreated += (o, e) =>
			{
				if (__instance.Created && !__instance.IsDisposed)
				{
					cType_OnAutoScaleModeChanged.Invoke(__instance, null);
					cType_LayoutScalingNeeded.Invoke(__instance, null);
				}
			};
		}

		public static void Form_SetFont(System.Windows.Forms.Form __instance)
		{
			Helper.Log($"Setting base Font from '{__instance.Font}' to '{DefaultFont}'.");
			__instance.Font = DefaultFont;
		}
	}
}
