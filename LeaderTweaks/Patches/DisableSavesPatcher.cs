using EoCPlugin;

using HarmonyLib;

using MessagePlugin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Disable Saves Patch")]
	public class DisableSavesPatcher : IPatcher
	{
		public void Init(Harmony patcher)
		{
			var pt = typeof(DisableSavesPatcher);
			var t = typeof(BrowserPluginHelper);

			patcher.Patch(AccessTools.Method(t, nameof(BrowserPluginHelper.SetupSaveGames)),
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
