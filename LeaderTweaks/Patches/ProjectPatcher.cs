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
using System.Windows.Controls.Primitives;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Project Settings Tweaks", "ProjectSettings")]
	public class ProjectPatcher : IPatcher
	{
		private static Dictionary<string, PropertyInfo> MetaStringAttributes;

		static readonly MethodInfo add_Click = AccessTools.Method(typeof(System.Windows.Forms.ToolStripItem), "add_Click");
		static readonly MethodInfo m_RefreshMeta = AccessTools.Method(typeof(ProjectPatcher), "RefreshMeta");
		static readonly MethodInfo m_ProjectAddClickEvent = AccessTools.Method(typeof(ProjectPatcher), nameof(ProjectPatcher.ProjectPlugin_AddClickEvent));
		static readonly MethodInfo m_PublishAddClickEvent = AccessTools.Method(typeof(ProjectPatcher), nameof(ProjectPatcher.PublishPlugin_AddClickEvent));
		static readonly MethodInfo m_ProjectOpenSettings = AccessTools.Method(typeof(ProjectPlugin), "OpenSettings");
		static readonly MethodInfo m_PublishCreatePanel = AccessTools.Method(typeof(PublishPlugin), nameof(PublishPlugin.CreatePanel));

		public void Init(Harmony harmony)
		{
			var pt = typeof(ProjectPatcher);

			harmony.Patch(AccessTools.Method(typeof(ProjectPlugin), nameof(ProjectPlugin.Start)),
				transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(ProjectPatcher.t_ProjectPlugin_Start))));

			harmony.Patch(AccessTools.Method(typeof(PublishPlugin), nameof(PublishPlugin.RegisterPanel)),
				transpiler: new HarmonyMethod(AccessTools.Method(pt, nameof(ProjectPatcher.t_PublishPlugin_RegisterPanel))));

			harmony.Patch(AccessTools.Method(typeof(PublishWindow), nameof(PublishWindow.GetSelectedWorkshopFeatureTags)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ProjectPatcher.GetSelectedWorkshopFeatureTags))));

			harmony.Patch(AccessTools.Method(typeof(PublishWindow), nameof(PublishWindow.SetSelectedWorkshopFeatureTags)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(ProjectPatcher.SetSelectedWorkshopFeatureTags))));

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
			for (int i = 0; i < code.Count; i++)
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
				code[insertAt - 2] = new CodeInstruction(OpCodes.Call, m_ProjectAddClickEvent);
				code[insertAt - 1] = new CodeInstruction(OpCodes.Nop);
				code[insertAt] = new CodeInstruction(OpCodes.Nop);
			}

			return code.AsEnumerable();
		}

		public static void ProjectPlugin_AddClickEvent(System.Windows.Forms.ToolStripItem toolStripItem, ProjectPlugin plugin)
		{
			Helper.Log("Added 'Click' event listener to project settings button.", false);
			toolStripItem.Click += (s, e) =>
			{
				if (LSToolFramework.ToolFramework.Instance != null && !String.IsNullOrWhiteSpace(LSToolFramework.ToolFramework.Instance.ModFolder))
				{
					RefreshMeta(s, e);
				}
				m_ProjectOpenSettings.Invoke(plugin, new object[] { s, e });
			};
		}

		public static IEnumerable<CodeInstruction> t_PublishPlugin_RegisterPanel(IEnumerable<CodeInstruction> instr)
		{
			var code = new List<CodeInstruction>(instr);
			int insertAt = -1;
			for (int i = 0; i < code.Count; i++)
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
				code[insertAt - 2] = new CodeInstruction(OpCodes.Call, m_PublishAddClickEvent);
				code[insertAt - 1] = new CodeInstruction(OpCodes.Nop);
				code[insertAt] = new CodeInstruction(OpCodes.Nop);
			}

			return code.AsEnumerable();
		}

		public static void PublishPlugin_AddClickEvent(System.Windows.Forms.ToolStripItem toolStripItem, PublishPlugin plugin)
		{
			Helper.Log("Added 'Click' event listener to publish button.", false);
			toolStripItem.Click += (s, e) =>
			{
				if (LSToolFramework.ToolFramework.Instance != null && !String.IsNullOrWhiteSpace(LSToolFramework.ToolFramework.Instance.ModFolder))
				{
					RefreshMeta(s, e);
				}
				m_PublishCreatePanel.Invoke(plugin, new object[] { s, e });
			};
		}

		//Reload mod settings before the settings window is opened
		public static void RefreshMeta(object sender, EventArgs e)
		{
			try
			{
				//EoCPlugin.ModBackend.Instance?.InitializeFromCurrentModServer();
				//FileLog.Log($"[LeaderTweaks] ActiveSettings[{EoCPlugin.ModBackend.Instance?.ActiveSettings}] VersionBuild[{EoCPlugin.ModBackend.Instance?.ActiveSettings?.VersionBuild}]");
				var settings = EoCPlugin.ModBackend.Instance?.ActiveSettings;
				string metaFilePath = LSToolFramework.ToolFramework.Instance.GameDataPath.Replace("/", "\\") + "Mods\\" + LSToolFramework.ToolFramework.Instance.ModFolder + "\\meta.lsx";
				Helper.Log($"Reloading meta.lsx at ({metaFilePath})", false);
				if (File.Exists(metaFilePath) && settings != null)
				{
					var root = XElement.Load(metaFilePath);
					if (root != null)
					{
						foreach (KeyValuePair<string, PropertyInfo> att in MetaStringAttributes)
						{
							var attValue = root.XPathSelectElement($"//node[@id='ModuleInfo']//attribute[@id='{att.Key}']")?.Attribute("value")?.Value;
							if (attValue != null)
							{
								att.Value.SetValue(settings, attValue, null);
							}
						}
						var version = root.XPathSelectElement("//node[@id='ModuleInfo']//attribute[@id='Version']")?.Attribute("value")?.Value;
						var numPlayers = root.XPathSelectElement("//node[@id='ModuleInfo']//attribute[@id='NumPlayers']")?.Attribute("value")?.Value;

						if (version != null && int.TryParse(version, out int vint))
						{
							settings.VersionMajor = (vint >> 28);
							settings.VersionMinor = (vint >> 24) & 0x0F;
							settings.VersionRevision = (vint >> 16) & 0xFF;
							settings.VersionBuild = (vint & 0xFFFF);
						}

						if (numPlayers != null && int.TryParse(numPlayers, out int num))
						{
							settings.NumPlayers = num;
						}

						Helper.Log("Updated project settings from meta.lsx", false);
					}
					else
					{
						Helper.Log("Failed to parse meta.lsx.", false);
					}
				}
				else
				{
					Helper.Log($"No file found at '${metaFilePath}'?", false);
				}
			}
			catch (Exception ex)
			{
				Helper.Log($"Error parsing meta.lsx: {ex}");
			}
		}

		public static void GetSelectedWorkshopFeatureTags(ref List<EWorkshopFeatureTags> __result, PublishWindow __instance)
		{
			var settings = EoCPlugin.ModBackend.Instance?.ActiveSettings;
			if (settings != null)
			{
				var t = typeof(EWorkshopFeatureTags);
				var tags = settings.Tags.Split(';').Select(x => x.ToLower()).ToList();
				foreach (string workshopTag in Enum.GetNames(t))
				{
					if (tags.Contains(workshopTag.ToLower()))
					{
						EWorkshopFeatureTags enumValue = (EWorkshopFeatureTags)Enum.Parse(t, workshopTag);
						if (!__result.Contains(enumValue))
						{
							__result.Add(enumValue);
						}
					}
				}
			}

			var t2 = typeof(EPublishTag);

			var projectTagsFixed = __instance.ProjectTags.Distinct().OrderBy(x => Enum.GetName(t2, x));
			__instance.ProjectTags.Clear();
			foreach (var tag in projectTagsFixed)
			{
				__instance.ProjectTags.Add(tag);
			}

			Helper.Log($"GetSelectedWorkshopFeatureTags Result: {String.Join(",", __result)}", false);
			Helper.Log($"GetSelectedWorkshopFeatureTags ProjectTags: {String.Join(",", __instance.ProjectTags)}", false);
		}

		public static void SetSelectedWorkshopFeatureTags(HashSet<EWorkshopFeatureTags> tags, PublishWindow __instance,
			ToggleButton ___m_ArmorsTag, ToggleButton ___m_BalancingStatsTag, ToggleButton ___m_ClassesTag,
			ToggleButton ___m_CompanionsTag, ToggleButton ___m_ConsumablesTag, ToggleButton ___m_MapsTag,
			ToggleButton ___m_OriginsTag, ToggleButton ___m_OverhaulsTag, ToggleButton ___m_QualityOfLifeTag,
			ToggleButton ___m_QuestsTag, ToggleButton ___m_RacesTag, ToggleButton ___m_RunesBoostsTag, ToggleButton ___m_SkillsTag,
			ToggleButton ___m_UtilityTag, ToggleButton ___m_VisualOverridesTag, ToggleButton ___m_WeaponsTag)
		{
			List<EWorkshopFeatureTags> foundTags = new List<EWorkshopFeatureTags>();
			var settings = EoCPlugin.ModBackend.Instance?.ActiveSettings;
			if (settings != null)
			{
				var t = typeof(EWorkshopFeatureTags);
				var stringTags = settings.Tags.Split(';').Select(x => x.ToLower()).ToList();
				foreach (string workshopTag in Enum.GetNames(t))
				{
					if (stringTags.Contains(workshopTag.ToLower()))
					{
						EWorkshopFeatureTags enumValue = (EWorkshopFeatureTags)Enum.Parse(t, workshopTag);
						if (!foundTags.Contains(enumValue))
						{
							foundTags.Add(enumValue);
						}
					}
				}
			}

			___m_ArmorsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Armors) : false;
			___m_BalancingStatsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.BalancingStats) : false;
			___m_ClassesTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Classes) : false;
			___m_CompanionsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Companions) : false;
			___m_ConsumablesTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Consumables) : false;
			___m_MapsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Maps) : false;
			___m_OriginsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Origins) : false;
			___m_OverhaulsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Overhauls) : false;
			___m_QualityOfLifeTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.QualityOfLife) : false;
			___m_QuestsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Quests) : false;
			___m_RacesTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Races) : false;
			___m_RunesBoostsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.RunesBoosts) : false;
			___m_SkillsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Skills) : false;
			___m_UtilityTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Utility) : false;
			___m_VisualOverridesTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.VisualOverrides) : false;
			___m_WeaponsTag.IsChecked = ___m_ArmorsTag.IsChecked != true ? foundTags.Contains(EWorkshopFeatureTags.Weapons) : false;

			var t2 = typeof(EPublishTag);

			var projectTagsFixed = __instance.ProjectTags.Distinct().OrderBy(x => Enum.GetName(t2, x));
			__instance.ProjectTags.Clear();
			foreach (var tag in projectTagsFixed)
			{
				__instance.ProjectTags.Add(tag);
			}

			Helper.Log($"SetSelectedWorkshopFeatureTags Result: {String.Join(",", foundTags)}", false);
			Helper.Log($"SetSelectedWorkshopFeatureTags ProjectTags: {String.Join(",", __instance.ProjectTags)}", false);
		}
	}
}
