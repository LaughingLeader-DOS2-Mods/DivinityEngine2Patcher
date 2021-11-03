using EoCPlugin;

using HarmonyLib;

using LSFrameworkPlugin;

using LSToolFramework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("RootTemplate Tweaks")]
	public class RootTemplatesPatcher : IPatcher
	{
		public void Init(Harmony harmony)
		{
			var pt = typeof(RootTemplatesPatcher);
			var rtp = typeof(RootTemplatePlugin);

			//harmony.Patch(AccessTools.Method(typeof(IPlugin), nameof(IPlugin.OnModuleLoaded)),
			//	prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(RootTemplatesPatcher.LoadRootTemplatesBeforeLevel))));

			harmony.Patch(AccessTools.Method(typeof(EoCPluginClass), nameof(EoCPluginClass.OnModuleLoaded)),
				prefix: new HarmonyMethod(AccessTools.Method(pt, nameof(RootTemplatesPatcher.LoadRootTemplatesBeforeLevel))));
		}

		// Makes the Root Templates panel load before a level is loaded, allowing you to edit/preview root templates without loading a level
		public static void LoadRootTemplatesBeforeLevel(object A_0, EventArgs A_1, EoCPluginClass __instance)
		{
			PanelService service = ToolFramework.Instance.ServiceManagerInstance.GetService<PanelService>();
			if (service != null)
			{
				RootTemplatePanel panel = service.GetPanel<RootTemplatePanel>();
				if (panel != null)
				{
					panel.Engine_OnStarted(A_0, A_1);
				}
			}
		}
	}
}
