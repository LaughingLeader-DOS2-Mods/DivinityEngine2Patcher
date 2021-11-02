
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private static string executableDirectory = "";

        [DllExport]
        public static void LoadEditorPatch()
		{
            Console.WriteLine("[LeaderTweaks] initializing...");
            var pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            executableDirectory = Directory.GetParentpluginDirectory).FullName;

            //Console.WriteLine(String.Join(";", assemblyDirectories));
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            harmony = new Harmony("laughingleader.leadertweaks");
            FileLog.logPath = Path.Combine(pluginDirectory, "harmony.log");
			System.IO.File.WriteAllText(FileLog.logPath, "");
			Harmony.DEBUG = true;
			harmony.CreateClassProcessor(typeof(Initializer)).Patch();

            var pt = typeof(IPatcher);

            Patchers = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.Namespace == "LeaderTweaks.Patches" && pt.IsAssignableFrom(t)).
                Select(t => Activator.CreateInstance(t)).Cast<IPatcher>().ToList();
            Console.WriteLine(String.Join(";", Patchers.Select(x => x.GetType().ToString())));
            Patchers.ForEach(LoadPatcher);
			FileLog.GetBuffer(true);
            Console.WriteLine("[LeaderTweaks] All patches enabled!");
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

            Console.WriteLine(logMessage);

            if (displayInGame)
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
