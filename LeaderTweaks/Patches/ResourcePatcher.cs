using HarmonyLib;

using LSFrameworkPlugin;

using LSToolFramework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LeaderTweaks.Patches
{
	[HarmonyPatch(typeof(Resource))]
	public static class ResourcePatcher
	{
		public static void Init(Harmony harmony)
		{
			var pt = typeof(ResourcePatcher);
			var t1 = typeof(Resource);
			var t3 = typeof(PhysicsResource);
			harmony.Patch(AccessTools.Method(t1, "Valid"), postfix: new HarmonyMethod(AccessTools.Method(pt, "Valid")));
			harmony.Patch(AccessTools.Method(t3, "ImageIndex"), postfix: new HarmonyMethod(AccessTools.Method(pt, "ImageIndex")));

			var publicFuncFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
			//Using the regular reflection method here since there's a static GetSingleFileName as well
			harmony.Patch(t1.GetMethod("GetSingleFileName", publicFuncFlags), 
				postfix: new HarmonyMethod(AccessTools.Method(pt, "GetSingleFileName")));
		}

		public static void GetSingleFileName(ref string __result, Resource __instance)
		{
			if (!String.IsNullOrWhiteSpace(__instance.FileName))
			{
				__result = __instance.FileName;
			}
		}

		//Fix for physics resources crashing upon previewing, if no level is loaded
		public static void Valid(ref bool __result, Resource __instance, bool ___m_Valid)
		{
			if (___m_Valid && String.IsNullOrEmpty(ToolFramework.Instance?.LevelDataPath) &&  __instance is PhysicsResource)
			{
				__result = false;
			} 
		}

		//Making sure the above result doesn't change the resource icon
		public static void ImageIndex(ref int __result, PhysicsResource __instance, bool ___m_Valid)
		{
			if (!___m_Valid)
			{
				__result = 0;
			}
			__result = 9;
		}
	}
}
