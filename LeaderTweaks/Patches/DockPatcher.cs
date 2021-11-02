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
	[LeaderPatcher("Docking Window Crap")]
	public class DockPatcher
	{
		public static void Init(Harmony harmony)
		{
			var pt = typeof(DockPatcher);
			var dcd = AccessTools.TypeByName("LSToolFramework.PanelService.DockContentDescription");

			harmony.Patch(AccessTools.PropertyGetter(dcd, "Title"),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(DockPatcher.GetDockTitle))));
		}

		//Shorten the Eyes of a Child text
		public static void GetDockTitle(ref string __result)
		{
			if (__result.Contains("Eyes of"))
			{
				__result = "Game";
			}
		}
	}
}
