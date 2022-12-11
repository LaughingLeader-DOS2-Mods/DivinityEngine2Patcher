using EoCPlugin;

using HarmonyLib;

using System.Collections.Generic;
using System.Reflection.Emit;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Disable Save Loading", "SaveLoading")]
	public class DisableSavesPatcher : IPatcher
	{
		public void Init(Harmony patcher)
		{
			var pt = typeof(DisableSavesPatcher);

			patcher.Patch(AccessTools.Method(typeof(BrowserPluginHelper), nameof(BrowserPluginHelper.SetupSaveGames)),
				transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(DisableSavesPatcher.t_DisableSaveModuleLoading))));
		}

		/* 
		 * Disables this function which *may* resuls in the editor trying to load all modules from your saves.
		*/
		public static IEnumerable<CodeInstruction> t_DisableSaveModuleLoading(IEnumerable<CodeInstruction> instr)
		{
			yield return new CodeInstruction(OpCodes.Ret);
		}
	}
}
