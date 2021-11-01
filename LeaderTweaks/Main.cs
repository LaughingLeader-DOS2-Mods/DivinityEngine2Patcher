
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LeaderTweaks.Patches;

using LSToolFramework;

namespace LeaderTweaks
{
    public static class Main
    {
        public static Harmony harmony { get; private set; }

        private static List<IPatcher> Patchers;

        [DllExport]
        public static void Init()
		{
            Console.WriteLine("INJECTED...");

            harmony = new Harmony("laughingleader.editortweaks");
            FileLog.logPath = @"C:\DOS2DE_Engine\DefEd\harmony.log";
            System.IO.File.WriteAllText(FileLog.logPath, "");
            Harmony.DEBUG = true;
            harmony.CreateClassProcessor(typeof(Initializer)).Patch();

            Patchers = new List<IPatcher>()
            {
                new AddResourceWizardPatcher(),
                new ResourcePatcher(),
                new ProjectPatcher(),
                new MessagePatcher(),
            };

            Patchers.ForEach(LoadPatcher);
            //FileLog.GetBuffer(true);
            Helper.Log("All patches enabled!");
        }

        static Type LeaderPatcherType = typeof(LeaderPatcherAttribute);

        static void LoadPatcher(IPatcher patcher)
		{
            string id = "";
            try
            {
                LeaderPatcherAttribute patcherDetails = Attribute.GetCustomAttribute(patcher.GetType(), LeaderPatcherType) as LeaderPatcherAttribute;
                if (patcherDetails != null)
                {
                    id = patcherDetails.ID;
                }
                patcher.Init(harmony);
                Helper.Log($"Initialized '{id}' patcher.");
            }
            catch(Exception ex)
			{
                Helper.Log($"Error initializing patcher [{id}]:\n{ex}");
            }
        }
    }

    public class Helper
	{
        public static void Log(string msg, bool displayInGame = true, [CallerMemberName] string memberName = "")
        {
            string logMessage = $"[LeaderTweaks] {msg}";
            if (!String.IsNullOrWhiteSpace(memberName))
            {
                FileLog.Log($"[LeaderTweaks:{memberName}] {msg}");
            }
            else
            {
                FileLog.Log(logMessage);
            }

            if(displayInGame)
			{
                LSToolFramework.ToolFramework.Instance?.MessageService?.PostInfoMessage(logMessage);
			}
        }
    }

    [HarmonyPatch]
    class Initializer
    {
        [HarmonyPatch(typeof(EoCPlugin.EoCPluginClass), "OnModuleLoaded", MethodType.Normal)]
        [HarmonyPostfix]
        public static void OnModuleLoaded()
        {
            Helper.Log("Module loaded.", false);
        }
    }
}
