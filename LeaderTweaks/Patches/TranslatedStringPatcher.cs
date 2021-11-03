using HarmonyLib;

using LSFrameworkPlugin;

using Stats.StatsDataModel.Stats.Fields;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaderTweaks.Patches
{
	[LeaderPatcher("Localization Tweaks", DebugOnly = true)]
	public class TranslatedStringPatcher : IPatcher
	{
		public void Init(Harmony harmony)
		{
			var pt = typeof(TranslatedStringPatcher);

			harmony.Patch(AccessTools.PropertySetter(typeof(MTranslatedString), nameof(MTranslatedString.Content)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(TranslatedStringPatcher.SetTranslatedStringContent))));

			harmony.Patch(AccessTools.Method(typeof(TranslatedStringKeyPlugin), nameof(TranslatedStringKeyPlugin.Import)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(TranslatedStringPatcher.OnTranslatedStringKeyPluginImport))));

			harmony.Patch(AccessTools.PropertyGetter(typeof(TranslatedStringStatObjectField), nameof(TranslatedStringStatObjectField.IsContentEmpty)),
				postfix: new HarmonyMethod(AccessTools.Method(pt, nameof(TranslatedStringPatcher.StatsEditorTSField_IsContentEmpty))));
		}

		public static void SetTranslatedStringContent(ref string content, MTranslatedString __instance)
		{
			Helper.Log($"[MTranslatedString:set_Content] value({content}) key({__instance.Key})");
		}

		public static void OnTranslatedStringKeyPluginImport(string path, List<string> keys, List<string> values, List<string> speakers, List<string> extraData,
			TranslatedStringKeyPlugin __instance)
		{
			Helper.Log($"[TranslatedStringKeyPlugin.Import] path({path}) keys({String.Join(";", keys)})");
		}

		// Skips creating translated string keys if the key value is one in a Larian mod (override the text in english.xml instead!)
		public static void StatsEditorTSField_IsContentEmpty(ref bool __result, TranslatedStringStatObjectField __instance)
		{
			var key = __instance.Key;
			Helper.Log($"[TranslatedStringStatObjectField:IsContentEmpty] key({key})");
		}
	}
}
