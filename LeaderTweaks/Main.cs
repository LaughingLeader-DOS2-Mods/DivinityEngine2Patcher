﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using EoCPlugin;

using HarmonyLib;

using LeaderTweaks.Patches;

using LSFrameworkPlugin;

using LSToolFramework;

using UiLibrary;

namespace LeaderTweaks
{
    public static class Main
    {
        public static Harmony harmony { get; private set; }

        private static List<IPatcher> Patchers;

        private static string executableDirectory = "";

        private static readonly Type LeaderPatcherType = typeof(LeaderPatcherAttribute);
        private static readonly Type IPatcherType = typeof(IPatcher);

        private static bool CanActivatePatcher(Type t)
		{
            if(t.IsClass && t.Namespace == "LeaderTweaks.Patches" && IPatcherType.IsAssignableFrom(t))
			{
                LeaderPatcherAttribute patcherDetails = Attribute.GetCustomAttribute(t, LeaderPatcherType) as LeaderPatcherAttribute;
                if (patcherDetails != null)
                {
#if !DEBUG
                    if(patcherDetails.DebugOnly)
					{
                        return false;
					}
#endif
                    return patcherDetails.Enabled;
                }
                return true;
            }
            return false;
		}

        [DllExport]
        public static void LoadEditorPatch()
		{
            Console.WriteLine("[LeaderTweaks] initializing...");
            var pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            executableDirectory = Directory.GetParent(pluginDirectory).FullName;

            //Console.WriteLine(String.Join(";", assemblyDirectories));
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            harmony = new Harmony("laughingleader.leadertweaks");
            FileLog.logPath = Path.Combine(pluginDirectory, "harmony.log");
			System.IO.File.WriteAllText(FileLog.logPath, "");
			Harmony.DEBUG = true;
			//harmony.CreateClassProcessor(typeof(Initializer)).Patch();
			//harmony.CreateClassProcessor(typeof(KeyboardEventHooks)).Patch();

            var pt = typeof(IPatcher);

            Patchers = Assembly.GetExecutingAssembly().GetTypes().Where(t => CanActivatePatcher(t)).
                Select(t => Activator.CreateInstance(t)).Cast<IPatcher>().ToList();
            Patchers.ForEach(LoadPatcher);
			FileLog.GetBuffer(true);
            Console.WriteLine("[LeaderTweaks] All patches enabled!");
		}

        static void LoadPatcher(IPatcher patcher)
		{
            string id = "";
            try
            {
                LeaderPatcherAttribute patcherDetails = Attribute.GetCustomAttribute(patcher.GetType(), LeaderPatcherType) as LeaderPatcherAttribute;
                if (patcherDetails != null)
                {
                    id = patcherDetails.ID;
                    if(patcherDetails.Enabled)
					{
                        patcher.Init(harmony);
                        Helper.Log($"Initialized '{id}' patcher.");
                    }
                }
                else
				{
                    patcher.Init(harmony);
                    Helper.Log($"Initialized '{id}' patcher.");
				}
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
        static readonly MethodInfo m_GenerateWwiseVoiceProject = AccessTools.Method(typeof(EoCPluginClass), "GenerateWwiseVoiceProject");
        static readonly FastInvokeHandler GenerateWwiseVoiceProject = HarmonyLib.MethodInvoker.GetHandler(m_GenerateWwiseVoiceProject);

        [HarmonyPatch(typeof(EoCPlugin.EoCPluginClass), "OnModuleLoaded", MethodType.Normal)]
        [HarmonyPostfix]
        public static void OnModuleLoaded(EoCPluginClass __instance)
        {
            Helper.Log("Module loaded.", false);

            //GenerateWwiseVoiceProject.Invoke(__instance, null, EventArgs.Empty);

            //EoCPluginClass plugin = MacrosHelper.GetPlugin<EoCPluginClass>();
        }
    }

    [HarmonyPatch(typeof(MNETWindowManager))]
    class KeyboardEventHooks
    {
        [HarmonyPatch("ReceiveKeyDownEvent")]
        [HarmonyPostfix]
        public static void ReceiveKeyDownEvent(System.Windows.Forms.KeyEventArgs e, MNETWindowManager __instance)
        {
            
        }

        [HarmonyPatch("ReceiveKeyUpEvent")]
        [HarmonyPostfix]
        public static void ReceiveKeyUpEvent(System.Windows.Forms.KeyEventArgs e, MNETWindowManager __instance)
        {
            
        }
	}
}
