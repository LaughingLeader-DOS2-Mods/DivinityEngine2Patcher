using HarmonyLib;

using EoCPlugin;
using EoCPluginCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher]
	public class ProjectPatcher : IPatcher
	{
		private static bool loaded = false;

		private static Dictionary<string, PropertyInfo> MetaStringAttributes;

		static readonly MethodInfo add_Click = AccessTools.Method(typeof(System.Windows.Forms.ToolStripItem), "add_Click");
		static readonly MethodInfo m_RefreshMeta = AccessTools.Method(typeof(ProjectPatcher), "RefreshMeta");
		static readonly MethodInfo m_ProjectAddClickEvent = AccessTools.Method(typeof(ProjectPatcher), nameof(ProjectPatcher.ProjectPlugin_AddClickEvent));
		static readonly MethodInfo m_PublishAddClickEvent = AccessTools.Method(typeof(ProjectPatcher), nameof(ProjectPatcher.PublishPlugin_AddClickEvent));
		static readonly MethodInfo m_ProjectOpenSettings = AccessTools.Method(typeof(ProjectPlugin), "OpenSettings");
		static readonly MethodInfo m_PublishCreatePanel = AccessTools.Method(typeof(ProjectPlugin), nameof(PublishPlugin.CreatePanel));

		public void Init(Harmony harmony)
		{
			if (loaded) return;

			var pt = typeof(ProjectPatcher);

			harmony.Patch(AccessTools.Method(typeof(ProjectPlugin), nameof(ProjectPlugin.Start)),
				transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(ProjectPatcher.t_ProjectPlugin_Start))));
			harmony.Patch(AccessTools.Method(typeof(PublishPlugin), nameof(PublishPlugin.RegisterPanel)),
				transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(ProjectPatcher.t_PublishPlugin_RegisterPanel))));
			loaded = true;

			var t2 = typeof(ModSettings);

			MetaStringAttributes = new Dictionary<string, PropertyInfo>()
			{
				{ "Tags", AccessTools.Property(t2, nameof(ModSettings.Tags))},
				{ "Name", AccessTools.Property(t2, nameof(ModSettings.Title))},
				{ "Description", AccessTools.Property(t2, nameof(ModSettings.Description))},
				{ "Author", AccessTools.Property(t2, nameof(ModSettings.Author))},
				{ "Type", AccessTools.Property(t2, nameof(ModSettings.Type))},
				{ "CharacterCreationLevelName", AccessTools.Property(t2, nameof(ModSettings.CharacterCreationLevel))},
				{ "MenuLevelName", AccessTools.Property(t2, nameof(ModSettings.MenuLevel))},
				{ "StartupLevelName", AccessTools.Property(t2, nameof(ModSettings.StartupLevel))},
				{ "PhotoBooth", AccessTools.Property(t2, nameof(ModSettings.PhotoBoothLevel))},
				{ "LobbyLevelName", AccessTools.Property(t2, nameof(ModSettings.LobbyLevel))},
				{ "GMTemplate", AccessTools.Property(t2, nameof(ModSettings.GMTemplate))},
				//{ "Version", AccessTools.Property(t2, nameof(ModSettings.Tags))},
				//{ "NumPlayers", AccessTools.Property(t2, nameof(ModSettings.NumPlayers))},
			};
		}


		//Hook into Start since it doesn't have any native code
		public static IEnumerable<CodeInstruction> t_ProjectPlugin_Start(IEnumerable<CodeInstruction> instr)
		{
			var code = new List<CodeInstruction>(instr);
			int insertAt = -1;
			for(int i = 0; i < code.Count; i++)
			{
				if (code[i].opcode == OpCodes.Call && (MethodInfo)code[i].operand == add_Click)
				{
					insertAt = i;
					break;
				}
			}
			if (insertAt > -1)
			{
				//Replacing the Click += OpenSettings code
				code[insertAt-2] = new CodeInstruction(OpCodes.Call, m_ProjectAddClickEvent);
				code[insertAt-1] = new CodeInstruction(OpCodes.Nop);
				code[insertAt] = new CodeInstruction(OpCodes.Nop);
			}
			
			return code.AsEnumerable();
		}

		public static void ProjectPlugin_AddClickEvent(System.Windows.Forms.ToolStripItem toolStripItem, ProjectPlugin plugin)
		{
			Helper.Log("Added 'Click' event listener to project settings button.", false);
			LSToolFramework.ToolFramework.Instance?.MessageService?.PostInfoMessage("[LeaderTweaks] Added 'Click' event listener to project settings button.");
			toolStripItem.Click += (s, e) =>
			{
				RefreshMeta(s, e);
				m_ProjectOpenSettings.Invoke(plugin, new object[] { s, e });
			};
		}

		public static IEnumerable<CodeInstruction> t_PublishPlugin_RegisterPanel(IEnumerable<CodeInstruction> instr)
		{
			var code = new List<CodeInstruction>(instr);
			int insertAt = -1;
			for(int i = 0; i < code.Count; i++)
			{
				if (code[i].opcode == OpCodes.Call && (MethodInfo)code[i].operand == add_Click)
				{
					insertAt = i;
					break;
				}
			}
			if (insertAt > -1)
			{
				//Replacing the Click += OpenSettings code
				code[insertAt-2] = new CodeInstruction(OpCodes.Call, m_PublishAddClickEvent);
				code[insertAt-1] = new CodeInstruction(OpCodes.Nop);
				code[insertAt] = new CodeInstruction(OpCodes.Nop);
			}
			
			return code.AsEnumerable();
		}

		public static void PublishPlugin_AddClickEvent(System.Windows.Forms.ToolStripItem toolStripItem, PublishPlugin plugin)
		{
			Helper.Log("Added 'Click' event listener to project settings button.", false);
			LSToolFramework.ToolFramework.Instance?.MessageService?.PostInfoMessage("[LeaderTweaks] Added 'Click' event listener to project settings button.");
			toolStripItem.Click += (s,e) =>
			{
				RefreshMeta(s, e);
				m_PublishCreatePanel.Invoke(plugin, new object[] { s, e });
			};
		}

		//Reload mod settings before the settings window is opened
		public static void RefreshMeta(object sender, EventArgs e)
		{
			try
			{
				LSToolFramework.ToolFramework.Instance?.MessageService?.PostInfoMessage("[LeaderTweaks] Reloading meta.lsx");
				//EoCPlugin.ModBackend.Instance?.InitializeFromCurrentModServer();
				//FileLog.Log($"[LeaderTweaks] ActiveSettings[{EoCPlugin.ModBackend.Instance?.ActiveSettings}] VersionBuild[{EoCPlugin.ModBackend.Instance?.ActiveSettings?.VersionBuild}]");
				var settings = EoCPlugin.ModBackend.Instance?.ActiveSettings;
				string metaFilePath = LSToolFramework.ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Mods\\" + LSToolFramework.ToolFramework.Instance.ModFolder + "\\meta.lsx";
				Helper.Log($"Reloading meta.lsx at ({metaFilePath})");
				if (File.Exists(metaFilePath) && settings != null)
				{
					var root = XElement.Load(metaFilePath);
					if (root != null)
					{
						foreach (KeyValuePair<string, PropertyInfo> att in MetaStringAttributes)
						{
							var attValue = root.XPathSelectElement($"//node[@id='ModuleInfo']//attribute[@id='{att.Key}']")?.Attribute("value");
							if (attValue != null)
							{
								att.Value.SetValue(settings, attValue, null);
							}
						}
						var version = root.XPathSelectElement("//node[@id='ModuleInfo']//attribute[@id='Version']")?.Attribute("value");
						var numPlayers = root.XPathSelectElement("//node[@id='ModuleInfo']//attribute[@id='NumPlayers']")?.Attribute("value");

						if (version != null && int.TryParse(version.Value, out int vint))
						{
							settings.VersionMajor = (vint >> 28);
							settings.VersionMinor = (vint >> 24) & 0x0F;
							settings.VersionRevision = (vint >> 16) & 0xFF;
							settings.VersionBuild = (vint & 0xFFFF);
						}

						if (numPlayers != null && int.TryParse(numPlayers.Value, out int num))
						{
							settings.NumPlayers = num;
						}

						Helper.Log("Updated project settings from meta.lsx");
					}
					else
					{
						Helper.Log("Failed to parse meta.lsx.");
					}
				}
				else
				{
					Helper.Log($"No file found at '${metaFilePath}'?");
				}
			}
			catch(Exception ex)
			{
				Helper.Log($"Error parsing meta.lsx:\n{ex}");
			}
		}
	}
}
