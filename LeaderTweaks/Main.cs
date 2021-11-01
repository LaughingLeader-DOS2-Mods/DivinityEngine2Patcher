
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LeaderTweaks.Patches;

using LSToolFramework;

namespace LeaderTweaks
{
    public static class Main
    {
        [DllExport]
        public static void Init()
		{
            Console.WriteLine("INJECTED...");

            var harmony = new Harmony("laughingleader.editortweaks");
            FileLog.logPath = @"C:\DOS2DE_Engine\DefEd\harmony.log";
            System.IO.File.WriteAllText(FileLog.logPath, "");
            Harmony.DEBUG = true;
            harmony.CreateClassProcessor(typeof(TestPatch)).Patch();
            //harmony.CreateClassProcessor(typeof(AddResourcePatch)).Patch();
            AddResourceWizardPatcher.Init(harmony);
            ResourcePatcher.Init(harmony);

            //FileLog.GetBuffer(true);
            FileLog.Log("[LeaderTweaks] Patches enabled!");
        }
    }

    [HarmonyPatch]
    class TestPatch
    {
        [HarmonyPatch(typeof(EoCPlugin.EoCPluginClass), "OnModuleLoaded", MethodType.Normal)]
        [HarmonyPostfix]
        public static void OnModuleLoaded()
        {
            var messageService = ToolFramework.Instance.MessageService;
            if(messageService != null)
			{
                messageService.PostInfoMessage("[LeaderTweaks] Leader Editor Tweaks is working!");
            }
        }
    }
}
